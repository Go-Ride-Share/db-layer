using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;

namespace GoRideShare.posts
{
    // This class handles making a new Post
    public class UpdatePost(ILogger<UpdatePost> logger)
    {
        private readonly ILogger<UpdatePost> _logger = logger;

        // This function is triggered by an HTTP PATCH request to create a new post
        [Function("PostsUpdate")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route ="Posts/{post_id}")] HttpRequest req, string post_id)
        {
            // Validate the post_id
            if (!Guid.TryParse(post_id, out Guid postId))
            {
                _logger.LogError("Invalid Query Parameter: `post_id` must be a Guid");
                return new BadRequestObjectResult("Invalid Query Parameter: `post_id` must be a Guid");
            } else {
                _logger.LogInformation($"post_id: {post_id}");
            }

            // Validate that the user has the required headers
            var validationResult = Utilities.ValidateHeaders(req.Headers, out Guid userId);
            if (validationResult != null)
            {
                _logger.LogError("Invalid Headers");
                return validationResult;
            } else {
                _logger.LogInformation($"userId: {userId}");
            }

            // Validate the required request body is present
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            PostDetails? updatedPost;
            try
            {
                updatedPost = JsonSerializer.Deserialize<PostDetails>(requestBody);

                if (updatedPost != null) {
                    var (invalid, errorMessage) = updatedPost.validate();
                    if (invalid)
                    {
                        _logger.LogError($"PostDetails are not valid: {errorMessage}");
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
                return new BadRequestObjectResult("Incomplete Post data.");
            }
            _logger.LogInformation($"Raw Request Body: {requestBody}");

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

                if ( !await CheckPostOwnership(connection, postId, userId) )
                {
                    return new BadRequestObjectResult("You cannot modify a post you did not create");
                }

                var query = """
                    UPDATE posts SET 
                        name = @Post_name,
                        origin_name = @Origin_name,
                        origin_lat = @Origin_lat,
                        origin_lng = @Origin_lng,
                        origin = POINT(@Origin_lat, @Origin_lng),
                        destination_name = @Destination_name,
                        destination_lat = @Destination_lat,
                        destination_lng = @Destination_lng,
                        destination = POINT(@Destination_lat, @Destination_lng),
                        description = @Description,
                        seats_available = @Seats_available
                    WHERE post_id = @Post_id
                """;

                // Use parameterized query to prevent SQL injection
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Post_id",          post_id);
                    command.Parameters.AddWithValue("@Post_name",        updatedPost.Name);
                    command.Parameters.AddWithValue("@Origin_name",      updatedPost.OriginName);
                    command.Parameters.AddWithValue("@Origin_lat",       updatedPost.OriginLat);
                    command.Parameters.AddWithValue("@Origin_lng",       updatedPost.OriginLng);
                    command.Parameters.AddWithValue("@Destination_name", updatedPost.DestinationName);
                    command.Parameters.AddWithValue("@Destination_lat",  updatedPost.DestinationLat);
                    command.Parameters.AddWithValue("@Destination_lng",  updatedPost.DestinationLng);
                    command.Parameters.AddWithValue("@Description",      updatedPost.Description);
                    command.Parameters.AddWithValue("@Seats_available",  updatedPost.SeatsAvailable);

                    try
                    {
                        await command.ExecuteNonQueryAsync();
                        _logger.LogInformation("Post Updated successfully.");
                        return new OkObjectResult(new { Id = post_id });
                    }
                    catch (MySqlException ex)
                    {
                        // Log the error if the query fails and return a 400 Bad Request response
                        _logger.LogError("Database error: " + ex.Message);
                        return new BadRequestObjectResult("Error Updaing post: " + ex.Message);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("An Unexpected Error Occured: " + ex.Message);
                        return new BadRequestObjectResult("An Error Occured: " + ex.Message);
                    }
                }
            }
        }
    
        private async Task<bool> CheckPostOwnership(MySqlConnection  connection, Guid postId, Guid posterId)
        {
            var query2 = "SELECT post_id, poster_id FROM posts WHERE post_id = @postId";
            using (var command = new MySqlCommand(query2, connection))
            {
                command.Parameters.AddWithValue("@postId", postId);
                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        if( !reader.IsDBNull(0) && !reader.IsDBNull(1))
                        {
                            Guid post_id = reader.GetGuid(0);
                            Guid poster_id = reader.GetGuid(1);
                            _logger.LogInformation($"post_id from DB: {post_id}");
                            _logger.LogInformation($"poster_id from DB: {poster_id}");
                            return post_id.Equals(postId) && poster_id.Equals(posterId);
                        }
                    }
                }
            }
            return false;
        }
    }
}
