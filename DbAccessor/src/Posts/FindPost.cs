using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;

namespace GoRideShare.posts
{
    // This class handles searching for a Post
    public class FindPost(ILogger<FindPost> logger)
    {
        private readonly ILogger<FindPost> _logger = logger;

        // This function is triggered by an HTTP GET request to create a new post
        [Function("PostsSearch")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Posts/Search")] HttpRequest req)
        {
            // Read the request body to get the 'Search Criteria'
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            SearchCriteria? searchCriteria;
            try
            {
                searchCriteria = JsonSerializer.Deserialize<SearchCriteria>(requestBody);
                if (searchCriteria != null) {
                    var (invalid, errorMessage) = searchCriteria.validate();
                    if (invalid)
                    {
                        _logger.LogError($"Search Criteria are not valid: {errorMessage}");
                        return new BadRequestObjectResult(errorMessage);
                    }
                } else {
                    _logger.LogError("Input was null");
                    return new BadRequestObjectResult("Input was null");
                }
            }
            catch (JsonException ex)
            {
                _logger.LogError($"JSON deserialization failed: {ex.Message}");
                return new BadRequestObjectResult("Incomplete Search Criteria.");
            }
            _logger.LogInformation($"Raw Request Body: {requestBody}");

            // Retrieve the database connection string from environment variables
            string? connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");
            using (var connection = new MySqlConnection(connectionString))
            {
                var (error, message) = await Utilities.ValidateConnection(connectionString, connection);
                if (error)
                {
                    _logger.LogError(message);
                    return new StatusCodeResult(StatusCodes.Status500InternalServerError);
                }

                // Use a parameterized query to fetch the posts
                var query = """
                    SELECT *
                    FROM (
                        SELECT *,
                            ST_Distance(origin, destination) AS org_distance,
                            ST_Distance(origin, POINT(@start_lat, @start_lng)) + ST_Distance(POINT(@start_lat, @start_lng), POINT(@end_lat, @end_lng)) + ST_Distance(POINT(@end_lat, @end_lng), destination) AS total_distance
                        FROM `go-ride-share`.posts
                    ) as distance_query
                    WHERE total_distance < 2 * org_distance
                    ORDER BY org_distance - total_distance DESC
                    LIMIT @Limit OFFSET @Offset;
                """;

                // Use parameterized query to reduce SQL injection
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@start_lat",   searchCriteria.OriginLat);
                    command.Parameters.AddWithValue("@start_lng",   searchCriteria.OriginLng);
                    command.Parameters.AddWithValue("@end_lat",     searchCriteria.DestinationLat);
                    command.Parameters.AddWithValue("@end_lng",     searchCriteria.DestinationLng);
                    command.Parameters.AddWithValue("@Limit",       searchCriteria.PageSize);
                    command.Parameters.AddWithValue("@Offset",      searchCriteria.PageStart);

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
                                        Name   = reader.GetString(reader.GetOrdinal("name")),
                                        Photo  = reader.GetString(reader.GetOrdinal("photo")),
                                    };

                                    Post post = new Post
                                    {
                                        PostId           = reader.GetGuid(  reader.GetOrdinal("post_id")),
                                        PosterId         = reader.GetGuid(  reader.GetOrdinal("poster_id")),
                                        Name             = reader.GetString(reader.GetOrdinal("name")),
                                        Description      = reader.GetString(reader.GetOrdinal("description")),
                                        DepartureDate    = reader.GetString(reader.GetOrdinal("departure_date")),
                                        OriginName       = reader.GetString(reader.GetOrdinal("origin_name")),
                                        OriginLat        = reader.GetFloat( reader.GetOrdinal("origin_lat")),
                                        OriginLng        = reader.GetFloat( reader.GetOrdinal("origin_lng")),
                                        DestinationName  = reader.GetString(reader.GetOrdinal("destination_name")),
                                        DestinationLat   = reader.GetFloat( reader.GetOrdinal("destination_lat")),
                                        DestinationLng   = reader.GetFloat( reader.GetOrdinal("destination_lng")),
                                        Price            = reader.GetFloat( reader.GetOrdinal("price")),
                                        SeatsAvailable   = reader.GetInt32( reader.GetOrdinal("seats_available")),
                                        SeatsTaken       = reader.GetInt32( reader.GetOrdinal("seats_taken")),
                                        Poster = poster
                                    };
                                    posts.Add(post);
                                } 
                                catch (Exception)
                                {
                                    //Shouldn't be possible, but invalid database entries can cause it.
                                    _logger.LogWarning($"Invalid post in DB"); 
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
