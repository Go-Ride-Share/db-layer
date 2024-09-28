using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;

namespace GoRideShare
{
    public class CreateAccount(ILogger<CreateAccount> logger)
    {
        private readonly ILogger<CreateAccount> _logger = logger;

        [Function("CreateUser")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var userToRegister = JsonSerializer.Deserialize<UserRegistrationInfo>(requestBody);

            _logger.LogInformation($"Raw Request Body: {JsonSerializer.Serialize(requestBody)}");

            if (userToRegister == null || string.IsNullOrEmpty(userToRegister.Email))
            {
                return new BadRequestObjectResult("Invalid user data.");
            }

            string? connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");
            using (var connection = new MySqlConnection(connectionString))
            {
                await connection.OpenAsync();

                // Use a parameterized query to insert the user data
                var query = "INSERT INTO users (user_id, email, password_hash, name, bio, preferences, phone_number, photo) " +
                            "VALUES (UUID(), @Email, @PasswordHash, @Name, @Bio, @Preferences, @PhoneNumber, @Photo)";

                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Email", userToRegister.Email);
                    command.Parameters.AddWithValue("@PasswordHash", userToRegister.PasswordHash);
                    command.Parameters.AddWithValue("@Name", userToRegister.Name);
                    command.Parameters.AddWithValue("@Bio", userToRegister.Bio);
                    command.Parameters.AddWithValue("@Preferences",JsonSerializer.Serialize(userToRegister.Preferences));
                    command.Parameters.AddWithValue("@PhoneNumber", userToRegister.PhoneNumber);
                    command.Parameters.AddWithValue("@Photo", userToRegister.Photo);

                    try
                    {
                        await command.ExecuteNonQueryAsync();
                        _logger.LogInformation("User created successfully.");
                        return new OkObjectResult("User created successfully.");
                    }
                    catch (MySqlException ex)
                    {
                        _logger.LogError("Database error: " + ex.Message);
                        return new BadRequestObjectResult("Error inserting user into the database.");
                    }
                }
            }
        }
    }
}
