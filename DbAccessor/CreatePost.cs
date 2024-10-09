using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;

namespace GoRideShare
{
    // This class handles making a new Post
    public class CreatePost(ILogger<CreatePost> logger)
    {
        private readonly ILogger<CreatePost> _logger = logger;

        // This function is triggered by an HTTP POST request to create a new post
        [Function("CreatePost")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req)
        {
            // Read the request body to get the post details
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var newPost = JsonSerializer.Deserialize<PostDetails>(requestBody);

            _logger.LogInformation($"Raw Request Body: {requestBody}");
            
            // Validate the required fields
            if (newPost == null || string.IsNullOrEmpty(newPost.Name) ||
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

                // Generate a new UUID for the new post
                string postId = Guid.NewGuid().ToString();

                // Use a parameterized query to insert the post details
                var query = "INSERT INTO posts (post_id, poster_id, name, origin_lat, origin_lng, destination_lat, destination_lng, description, seats_available, departure_date, price) " +
                            "VALUES (@Post_id, @Poster_id, @Name, @Origin_lat, @Origin_lng, @Destination_lat, @Destination_lng, @Description, @Seats_available, @Departure_date,@Price)";

                 // Use parameterized query to prevent SQL injection
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Post_id",         postId);
                    command.Parameters.AddWithValue("@Poster_id",       newPost.PosterId);
                    command.Parameters.AddWithValue("@Name",            newPost.Name);
                    command.Parameters.AddWithValue("@Origin_lat",      newPost.OriginLat);
                    command.Parameters.AddWithValue("@Origin_lng",      newPost.OriginLng);
                    command.Parameters.AddWithValue("@Destination_lat", newPost.DestinationLat);
                    command.Parameters.AddWithValue("@Destination_lng", newPost.DestinationLng);
                    command.Parameters.AddWithValue("@Description",     newPost.Description);
                    command.Parameters.AddWithValue("@Seats_available", newPost.SeatsAvailable);
                    command.Parameters.AddWithValue("@Departure_date",  newPost.DepartureDate);
                    command.Parameters.AddWithValue("@Price",           newPost.Price);

                    try
                    {
                        await command.ExecuteNonQueryAsync();
                        _logger.LogInformation("Posted successfully.");
                        return new OkObjectResult(new { Post_id = postId });
                    }
                    catch (MySqlException ex)
                    {
                        // Log the error if the query fails and return a 400 Bad Request response
                        _logger.LogError("Database error: " + ex.Message);
                        return new BadRequestObjectResult("Error inserting post into the database: " + ex.Message);
                    }
                }
            }
        }
    }
}
