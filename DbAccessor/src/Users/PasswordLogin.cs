using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using System.Text.Json;

namespace GoRideShare.users
{
    // This class handles login credential verification
    public class LoginUser(ILogger<LoginUser> logger)
    {
        private readonly ILogger<LoginUser> _logger = logger;

        // This function is triggered by an HTTP POST request
        [Function("PasswordLogin")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Users/PasswordLogin")] HttpRequest req)
        {
            // Read the request body to get the user's login data (email and password hash)
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var userToLogin = JsonSerializer.Deserialize<PasswordLoginCredentials>(requestBody);

            // Check if user data is missing or invalid
            if (userToLogin == null || string.IsNullOrEmpty(userToLogin.Email) || string.IsNullOrEmpty(userToLogin.PasswordHash))
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
                                var storedUserId = Utilities.GetUserIdFromReader(reader, ordinal: 0);
                                var storedPasswordHash = reader.IsDBNull(1) ? null : reader.GetString(1);
                                var storedPhoto = reader.IsDBNull(2) ? null : reader.GetString(2);

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
