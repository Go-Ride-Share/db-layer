using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;

namespace GoRideShare
{
    // This class handles making a new Post
    public class UpdatePost(ILogger<UpdatePost> logger)
    {
        private readonly ILogger<UpdatePost> _logger = logger;

        // This function is triggered by an HTTP PATCH request to create a new post
        [Function("UpdatePost")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "patch")] HttpRequest req)
        {
            // Read the request body to get the post details
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var newPost = JsonSerializer.Deserialize<PostDetails>(requestBody);

            _logger.LogInformation($"Raw Request Body: {requestBody}");
            
            // Validate the required fields
            if (newPost == null || string.IsNullOrEmpty(newPost.Name) ||
                string.IsNullOrEmpty(newPost.PostId) ||   
                string.IsNullOrEmpty(newPost.Description) || 
                string.IsNullOrEmpty(newPost.PosterId) || 
                90 < newPost.OriginLat || newPost.OriginLat < -90 ||
                90 < newPost.DestinationLat || newPost.DestinationLat < -90 ||
                180 < newPost.OriginLng || newPost.OriginLng < -180 ||
                180 < newPost.DestinationLng || newPost.DestinationLng < -180)
            {
                return new BadRequestObjectResult("Incomplete post data.");
            }

            // Retrieve the database connection string from environment variables
            string? connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");
            using (var connection = new MySqlConnection(connectionString))
            {
                // Validate the connection string before trying to open the connection
                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    _logger.LogError("Invalid connection string.");
                    return new StatusCodeResult(StatusCodes.Status500InternalServerError);
                }

                try
                {
                    // open the connection with the database
                    await connection.OpenAsync();
                }
                catch (MySqlException ex)
                {
                    // Log the error and return an appropriate response
                    _logger.LogError($"Failed to open database connection: {ex.Message}");
                    return new StatusCodeResult(StatusCodes.Status500InternalServerError);
                }

                var query = """
                    UPDATE posts SET 
                    origin_lat = @Origin_lat,
                    origin_lng = @Origin_lng,
                    destination_lat = @Destination_lat,
                    destination_lng = @Destination_lng,
                    description = @Description,
                    seats_available = @Seats_available
                    WHERE post_id = @Post_id AND poster_id = @Poster_id
                """;

                 // Use parameterized query to prevent SQL injection
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Post_id",         newPost.PostId);
                    command.Parameters.AddWithValue("@Poster_id",       newPost.PosterId);
                    command.Parameters.AddWithValue("@Origin_lat",      newPost.OriginLat);
                    command.Parameters.AddWithValue("@Origin_lng",      newPost.OriginLng);
                    command.Parameters.AddWithValue("@Destination_lat", newPost.DestinationLat);
                    command.Parameters.AddWithValue("@Destination_lng", newPost.DestinationLng);
                    command.Parameters.AddWithValue("@Description",     newPost.Description);
                    command.Parameters.AddWithValue("@Seats_available", newPost.SeatsAvailable);

                    try
                    {
                        await command.ExecuteNonQueryAsync();
                        _logger.LogInformation("Post Updated successfully.");
                        return new OkObjectResult(new { Post_id = newPost.PostId });
                    }
                    catch (MySqlException ex)
                    {
                        // Log the error if the query fails and return a 400 Bad Request response
                        _logger.LogError("Database error: " + ex.Message);
                        return new BadRequestObjectResult("Error Updaing post: " + ex.Message);
                    }
                }
            }
        }
    }
}
