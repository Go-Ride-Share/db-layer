using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace GoRideShare
{
    public class VerifyLoginCredentials(ILogger<VerifyLoginCredentials> logger)
    {
        private readonly ILogger<VerifyLoginCredentials> _logger = logger;

        [Function("VerifyLoginCredentials")]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
        {
            LoginCredentials loginCredentials = new("some email", "cc3f4fd9608d575655ed31844b2349cf37be8ec5e4b0ec8ba9994fbc6653666f");
            string json = JsonSerializer.Serialize<LoginCredentials>(loginCredentials);

            _logger.LogInformation("C# HTTP trigger function processed a request.");
            return new ContentResult
            {

                return new BadRequestObjectResult("Incomplete user data.");
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

                // Query to check if the email exists in the database and retrieve the password hash
                var query = "SELECT user_id, password_hash, photo FROM users WHERE email = @Email";

                // Execute the SQL query using a parameterized command to prevent SQL injection
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Email", userToLogin.Email.ToLower());

                    try
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                var storedUserId = reader.IsDBNull(0) ? null : reader.GetGuid(0).ToString();
                                var storedPasswordHash = reader.IsDBNull(1) ? null : reader.GetString(1);
                                var storedPhoto = reader.IsDBNull(2) ? null : reader.GetString(2); ;

                                // Check if password and username exist, and passwords match
                                if (storedPasswordHash == null || storedUserId == null || storedPasswordHash != userToLogin.PasswordHash)
                                    return new ObjectResult("Invalid login credentials.")
                                    {
                                        StatusCode = StatusCodes.Status401Unauthorized
                                    };

                                // If the password is correct, return 200 OK
                                return new OkObjectResult(new { User_id = storedUserId, Photo = storedPhoto });
                            }
                            else
                            {
                                // Return 401 Unauthorized if no user is found
                                return new ObjectResult("Invalid login credentials.")
                                {
                                    StatusCode = StatusCodes.Status401Unauthorized
                                };
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
