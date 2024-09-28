using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using System.Text.Json;

namespace GoRideShare
{
    public class VerifyLoginCredentials(ILogger<VerifyLoginCredentials> logger)
    {
        private readonly ILogger<VerifyLoginCredentials> _logger = logger;

        [Function("VerifyLoginCredentials")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var userToLogin = JsonSerializer.Deserialize<LoginCredentials>(requestBody);

            if (userToLogin == null || string.IsNullOrEmpty(userToLogin.Email))
            {
                return new BadRequestObjectResult("Invalid data.");
            }

            string? connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");
            using (var connection = new MySqlConnection(connectionString))
            {
                await connection.OpenAsync();

                var query = "SELECT password_hash FROM users WHERE email = @Email";

                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Email", userToLogin.Email);

                    try
                    {
                        var storedPasswordHash = (string?)await command.ExecuteScalarAsync();
                        
                        if (storedPasswordHash == null)
                        {
                            return new UnauthorizedResult();
                        }

                        if (storedPasswordHash != userToLogin.PasswordHash)
                        {
                            return new UnauthorizedResult();
                        }
                        
                        return new OkObjectResult("User logged in successfully.");

                    }
                    catch (MySqlException ex)
                    {
                        _logger.LogError("Database error: " + ex.Message);
                        return new BadRequestObjectResult("Error querying the database.");
                    }
                }
            }
        }
    }
}
