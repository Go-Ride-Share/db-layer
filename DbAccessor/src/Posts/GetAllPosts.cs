using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;

namespace GoRideShare
{
    // This class handles making a new Post
    public class GetAllPosts(ILogger<GetAllPosts> logger)
    {
        private readonly ILogger<GetAllPosts> _logger = logger;

        // This function is triggered by an HTTP GET request to fetch a users posts
        [Function("GetAllPosts")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
        {

            // Retrieve the database connection string from environment variables
            string? connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");
            using (var connection = new MySqlConnection(connectionString))
            {
                // Validate the connection string before trying to open the connection
                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    _logger.LogError("Invalid connection string.");
                    return new ObjectResult("Invalid database credentials.")
                    {
                        StatusCode = StatusCodes.Status401Unauthorized
                    };
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
                    return new ObjectResult("Failed to open database connection.")
                    {
                        StatusCode = StatusCodes.Status500InternalServerError
                    };
                }

                var query = "SELECT * FROM posts";
                using (var command = new MySqlCommand(query, connection))
                {
                    try
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            var posts = new List<object>();
                            while (await reader.ReadAsync())
                            {
                                try{
                                    // Connector/Net 6.1.1 and later automatically treat char(36) as a Guid type
                                    string storedPosterId;
                                    int ordinal = reader.GetOrdinal("poster_id");
                                    if (reader[ordinal].GetType() == typeof(Guid))
                                        storedPosterId = reader.GetGuid(ordinal).ToString();
                                    else
                                        storedPosterId = reader.GetString(ordinal);

                                    var post = new PostDetails
                                    {
                                        PostId          = reader.GetGuid(  reader.GetOrdinal("post_id")),
                                        PosterId        = storedPosterId,
                                        Name            = reader.GetString(reader.GetOrdinal("name")),
                                        Description     = reader.GetString(reader.GetOrdinal("description")),
                                        DepartureDate   = reader.GetString(reader.GetOrdinal("departure_date")),
                                        OriginLat       = reader.GetFloat( reader.GetOrdinal("origin_lat")),
                                        OriginLng       = reader.GetFloat( reader.GetOrdinal("origin_lng")),
                                        DestinationLat  = reader.GetFloat( reader.GetOrdinal("destination_lat")),
                                        DestinationLng  = reader.GetFloat( reader.GetOrdinal("destination_lng")),
                                        Price           = reader.GetFloat( reader.GetOrdinal("price")),
                                        SeatsAvailable  = reader.GetInt32( reader.GetOrdinal("seats_available"))
                                    };
                                    posts.Add(post);
                                } 
                                catch (Exception e)
                                {
                                    // Shouldn't be possible, but invalid database entries can cause it.
                                    _logger.LogWarning($"Invalid post in DB: {e.Message}"); 
                                }
                            }
                            _logger.LogInformation("Posts retrieved successfully.");
                            return new OkObjectResult(posts);
                        }
                    }
                    catch (MySqlException ex)
                    {
                        // Log the error if the query fails and return a 400 Bad Request response
                        _logger.LogError("Database error: " + ex.Message);
                        return new BadRequestObjectResult("Error fetching posts from the database: " + ex.Message);
                    }
                    catch (Exception ex)
                    {
                        // Log the error if the query fails and return a 400 Bad Request response
                        _logger.LogError("An Unexpected Error Occured: " + ex.Message);
                        return new BadRequestObjectResult("An Error Occured: " + ex.Message);
                    }
                }
            }
        }
    }
}
