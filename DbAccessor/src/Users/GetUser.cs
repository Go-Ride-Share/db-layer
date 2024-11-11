using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;

namespace GoRideShare.users
{
    public class GetUser(ILogger<GetUser> logger)
    {
        private readonly ILogger<GetUser> _logger = logger;

        [Function("UserGet")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Users/{user_id}")] HttpRequest req, Guid userId)
        {
            /*
            // If validation result is not null, return the bad request result
            var validationResult = Utilities.ValidateHeaders(req.Headers, out Guid userId);
            if (validationResult != null)
            {
                _logger.LogError("Invalid Headers");
                return validationResult;
            }
            */
            
            // Retrieve the database connection string from environment variables
            string? connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");
            using (var connection = new MySqlConnection(connectionString))
            {
                // Validate the connection string
                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    _logger.LogError("Invalid connection string.");
                    return new StatusCodeResult(StatusCodes.Status500InternalServerError);
                }

                try
                {
                    // Open the connection with the database
                    await connection.OpenAsync();
                }
                catch (MySqlException ex)
                {
                    // Log the error and return an appropriate response
                    _logger.LogError($"Failed to open database connection: {ex.Message}");
                    return new StatusCodeResult(StatusCodes.Status500InternalServerError);
                }

                // Query to retrieve user information by user ID
                var query = "SELECT email, bio, name, phone_number, photo FROM users WHERE user_id = @UserId";

                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserId", userId.ToString());

                    try
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                var email = reader.IsDBNull(0) ? null : reader.GetString(0);
                                var bio = reader.IsDBNull(1) ? null : reader.GetString(1);
                                var name = reader.IsDBNull(2) ? null : reader.GetString(2);
                                var phone = reader.IsDBNull(3) ? null : reader.GetString(3);
                                var photo = reader.IsDBNull(4) ? null : reader.GetString(4);

                                // Return 200 OK with user information
                                return new OkObjectResult(new { Email = email, Bio = bio, Name = name, Phone = phone, Photo = photo });
                            }
                            else
                            {
                                // Return 404 Not Found if no user is found
                                return new NotFoundResult();
                            }
                        }
                    }
                    catch (MySqlException ex)
                    {
                        // Log any database errors and return a 400 Bad Request with the error message
                        _logger.LogError("Database error: " + ex.Message);
                        return new BadRequestObjectResult("Error querying the database: " + ex.Message);
                    }
                }
            }
        }
    }
}