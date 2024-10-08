using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;

namespace GoRideShare
{
    // This class handles making a new Post
    public class GetPosts(ILogger<GetPosts> logger)
    {
        private readonly ILogger<GetPosts> _logger = logger;

        // This function is triggered by an HTTP POST request to create a new post
        [Function("GetPosts")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
        {

            // Read the request body to get the user's registration information
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var getPostsRequest = JsonSerializer.Deserialize<getPostsRequest>(requestBody);
            _logger.LogInformation($"Raw Request Body: {requestBody}");

            // Validate if essential user data is present
            if (getPostsRequest == null || string.IsNullOrEmpty(getPostsRequest.UserId))
            {
                return new BadRequestObjectResult("User Id invalid .");
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

                // Use a parameterized query to insert the post details
                var query = "SELECT * FROM posts WHERE poster_id = @Poster_id";

                 // Use parameterized query to prevent SQL injection
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Poster_id", getPostsRequest.UserId);

                    try
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            var posts = new List<object>();
                            while (await reader.ReadAsync())
                            {
                                var post = new PostDetails
                                {
                                    PostId = reader["post_id"].ToString(),
                                    PosterId = reader["poster_id"].ToString(),
                                    Name = reader["name"].ToString(),
                                    Description = reader["description"].ToString(),
                                    OriginLat = reader.GetFloat(reader.GetOrdinal("origin_lat")),
                                    OriginLng = reader.GetFloat(reader.GetOrdinal("origin_lng")),
                                    DestinationLat = reader.GetFloat(reader.GetOrdinal("destination_lat")),
                                    DestinationLng = reader.GetFloat(reader.GetOrdinal("destination_lng")),
                                    Price = reader.GetFloat(reader.GetOrdinal("price")),
                                    SeatsAvailable = reader.GetInt32(reader.GetOrdinal("seats_available"))
                                };
                                posts.Add(post);
                            }
                            _logger.LogInformation("Posts retrieved successfully.");
                            return new OkObjectResult(posts);
                        }
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
