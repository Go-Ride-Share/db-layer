using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using Microsoft.Extensions.Primitives;

namespace GoRideShare.posts
{
    // This class handles making a new Post
    public class GetPosts(ILogger<GetPosts> logger)
    {
        private readonly ILogger<GetPosts> _logger = logger;

        // This function is triggered by an HTTP GET request to fetch a users posts
        [Function("PostsGet")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Posts/{user_id?}")] HttpRequest req, string? user_id)
        {
            if ( user_id != null && !Guid.TryParse(user_id, out Guid _))
            {
                _logger.LogError("Invalid Query Parameter: `user_id` must be a Guid");
                return new BadRequestObjectResult("Invalid Query Parameter: `user_id` must be a Guid");
            } else {
                _logger.LogInformation($"user_id: {user_id}");
            }

            string? post_id = null;
            if (req.Query.TryGetValue("post_id", out StringValues postIdParam))
            {
                Guid post_guid = Guid.Empty;
                if (!Guid.TryParse(postIdParam[0], out post_guid))
                {
                    _logger.LogError("Invalid post_id query param");
                    return new BadRequestObjectResult("ERROR: Invalid Query Parameter: post_id");
                } else {
                    post_id = post_guid.ToString();
                }
            }

            // Pagination settings
            int pageStart = 0;
            int pageSize = 50;
            if (req.Query.TryGetValue("pageStart", out StringValues pageStartParam))
            {
                if (!int.TryParse(pageStartParam[0], out pageStart))
                {
                    _logger.LogError("Invalid pageStart query param");
                    return new BadRequestObjectResult("ERROR: Invalid Query Parameter: pageStart");
                }
            }
            if (req.Query.TryGetValue("pageSize", out StringValues pageSizeParam))
            {
                if (!int.TryParse(pageSizeParam[0], out pageSize)) 
                {
                    _logger.LogError("Invalid pageSize query param");
                    return new BadRequestObjectResult("ERROR: Invalid Query Parameter: pageSize");
                }
            }
            _logger.LogInformation($"pageStart: {pageStart}");
            _logger.LogInformation($"pageSize: {pageSize}");

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
                                users.photo as photo FROM posts join users on poster_id = user_id
                                WHERE user_id is not null
                                ";
                if (user_id != null) {
                    query += " AND poster_id = @Poster_id";
                }
                if (post_id != null) {
                    query += " AND post_id = @Post_id";
                }                
                query += " LIMIT @Limit OFFSET @Offset;";
                _logger.LogInformation(query);

                // Use parameterized query to prevent SQL injection
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Poster_id", user_id);
                    command.Parameters.AddWithValue("@Post_id", post_id);
                    command.Parameters.AddWithValue("@Limit", pageSize);
                    command.Parameters.AddWithValue("@Offset", pageStart);
                    try
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            var posts = new List<object>();
                            while (await reader.ReadAsync())
                            {
                                try{
                                    User poster = new User
                                    {
                                        UserId = reader.GetGuid(  reader.GetOrdinal("user_id")),
                                        Name   = reader.GetString(reader.GetOrdinal("user_name")),
                                        
                                        // Optional field may be null
                                        Photo  = !reader.IsDBNull(reader.GetOrdinal("photo")) ? reader.GetString(reader.GetOrdinal("photo")) : null,
                                    };

                                    Post post = new Post
                                    {
                                        PostId          = reader.GetGuid(  reader.GetOrdinal("post_id")),
                                        PosterId        = reader.GetGuid(  reader.GetOrdinal("poster_id")),
                                        Name            = reader.GetString(reader.GetOrdinal("name")),
                                        Description     = reader.GetString(reader.GetOrdinal("description")),
                                        DepartureDate   = reader.GetString(reader.GetOrdinal("departure_date")),
                                        Price           = reader.GetFloat( reader.GetOrdinal("price")),
                                        SeatsAvailable  = reader.GetInt32( reader.GetOrdinal("seats_available")),
                                        OriginLat       = reader.GetFloat( reader.GetOrdinal("origin_lat")),
                                        OriginLng       = reader.GetFloat( reader.GetOrdinal("origin_lng")),
                                        DestinationLat  = reader.GetFloat( reader.GetOrdinal("destination_lat")),
                                        DestinationLng  = reader.GetFloat( reader.GetOrdinal("destination_lng")),
                                        CreatedAt       = reader.GetDateTime(reader.GetOrdinal("created_at")),
                                        Poster = poster,

                                        // Optional fields may be null
                                        OriginName      = !reader.IsDBNull(reader.GetOrdinal("origin_name")) ? reader.GetString(reader.GetOrdinal("origin_name")) : null,
                                        DestinationName = !reader.IsDBNull(reader.GetOrdinal("destination_name")) ? reader.GetString(reader.GetOrdinal("destination_name")) : null,
                                    };
                                    posts.Add(post);
                                }
                                catch (Exception e)
                                {
                                    //Shouldn't be possible, but invalid database entries can cause it.
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
                        _logger.LogError("An Unexpected Error Occured: " + ex.Message);
                        return new BadRequestObjectResult("An Error Occured: " + ex.Message);
                    }
                }
            }
        }
    
    }
}
