using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using Microsoft.Extensions.Primitives;

namespace GoRideShare.posts
{
    // This class handles making a new Post
    public class GetPost(ILogger<GetPost> logger)
    {
        private readonly ILogger<GetPost> _logger = logger;

        // This function is triggered by an HTTP GET request to fetch a users posts
        [Function("PostGet")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Post/{post_id?}")] HttpRequest req, string? post_id)
        {
            if ( post_id != null && !Guid.TryParse(post_id, out Guid _))
            {
                _logger.LogError("Invalid Query Parameter: `post_id` must be a Guid");
                return new BadRequestObjectResult("Invalid Query Parameter: `post_id` must be a Guid");
            } else {
                _logger.LogInformation($"post_id: {post_id}");
            }


            // Retrieve the database connection string from environment variables
            string? connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");
            using (var connection = new MySqlConnection(connectionString))
            {
                // Validate the connection string before trying to open the connection
                var (error, message) = await Utilities.ValidateConnection(connectionString, connection);
                if (error)
                {
                    _logger.LogError(message);
                    return new StatusCodeResult(StatusCodes.Status500InternalServerError);
                }

                // Use a parameterized query to fetch the post details
                var query = @"SELECT 
                                posts.*, 
                                users.user_id as user_id, 
                                users.name as user_name,
                                users.photo as photo FROM posts join users on poster_id = user_id WHERE post_id = @Post_id";

                _logger.LogInformation(query);

                // Use parameterized query to prevent SQL injection
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Post_id", post_id);

                    try
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            Post? returnPost = null;
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

                                    User poster = new User
                                    {
                                        UserId = reader.GetGuid(  reader.GetOrdinal("user_id")),
                                        Name   = reader.GetString(reader.GetOrdinal("user_name")),
                                        
                                        // Optional field may be null
                                        Photo  = !reader.IsDBNull(reader.GetOrdinal("photo")) ? reader.GetString(reader.GetOrdinal("photo")) : null,
                                    };

                                    returnPost = new Post
                                    {
                                        PostId          = reader.GetGuid(  reader.GetOrdinal("post_id")),
                                        PosterId        = storedPosterId,
                                        Name            = reader.GetString(reader.GetOrdinal("name")),
                                        Description     = reader.GetString(reader.GetOrdinal("description")),
                                        DepartureDate   = reader.GetString(reader.GetOrdinal("departure_date")),
                                        Price           = reader.GetFloat( reader.GetOrdinal("price")),
                                        SeatsAvailable  = reader.GetInt32( reader.GetOrdinal("seats_available")),
                                        OriginLat       = reader.GetFloat( reader.GetOrdinal("origin_lat")),
                                        OriginLng       = reader.GetFloat( reader.GetOrdinal("origin_lng")),
                                        DestinationLat  = reader.GetFloat( reader.GetOrdinal("destination_lat")),
                                        DestinationLng  = reader.GetFloat( reader.GetOrdinal("destination_lng")),
                                        Price           = reader.GetFloat( reader.GetOrdinal("price")),
                                        SeatsAvailable  = reader.GetInt32( reader.GetOrdinal("seats_available"))
                                    };
                                    posts.Add(post);
                                } 
                                catch (Exception)
                                {
                                    //Shouldn't be possible, but invalid database entries can cause it.
                                    _logger.LogWarning($"Invalid post in DB"); 
                                }
                            }
                            _logger.LogInformation("Post retrieved successfully.");
                            return new OkObjectResult(returnPost);
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
                        _logger.LogError("An Unexpected Error Occured: " + ex.Message);
                        return new BadRequestObjectResult("An Error Occured: " + ex.Message);
                    }
                }
            }
        }
    
    }
}
