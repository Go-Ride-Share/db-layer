using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;

namespace GoRideShare
{
    // This class handles creating a new user account
    public class CreateAccount(ILogger<CreateAccount> logger)
    {
        private readonly ILogger<CreateAccount> _logger = logger;

        // This function is triggered by an HTTP POST request to create a user
        [Function("CreateUser")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req)
        {
            // Read the request body to get the user's registration information
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var userToRegister = JsonSerializer.Deserialize<UserRegistrationInfo>(requestBody);

            _logger.LogInformation($"Raw Request Body: {requestBody}");

            // Validate if essential user data is present
            if (userToRegister == null || string.IsNullOrEmpty(userToRegister.Email) ||
            string.IsNullOrEmpty(userToRegister.Name) || string.IsNullOrEmpty(userToRegister.PasswordHash)
            || string.IsNullOrEmpty(userToRegister.PhoneNumber))
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

                // Use a parameterized query to insert the user data
                var query = "INSERT INTO users (user_id, email, password_hash, name, bio, preferences, phone_number, photo) " +
                            "VALUES (UUID(), @Email, @PasswordHash, @Name, @Bio, @Preferences, @PhoneNumber, @Photo)";

                 // Use parameterized query to prevent SQL injection
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Email", userToRegister.Email);
                    command.Parameters.AddWithValue("@PasswordHash", userToRegister.PasswordHash);
                    command.Parameters.AddWithValue("@Name", userToRegister.Name);
                    command.Parameters.AddWithValue("@Bio", userToRegister.Bio);
                    command.Parameters.AddWithValue("@Preferences", JsonSerializer.Serialize(userToRegister.Preferences));
                    command.Parameters.AddWithValue("@PhoneNumber", userToRegister.PhoneNumber);
                    command.Parameters.AddWithValue("@Photo", userToRegister.Photo);

                    try
                    {
                        await command.ExecuteNonQueryAsync();
                        _logger.LogInformation("User created successfully.");
                        return new OkObjectResult("User account created successfully.");
                    }
                    catch (MySqlException ex)
                    {
                        // Log the error if the query fails and return a 400 Bad Request response
                        _logger.LogError("Database error: " + ex.Message);
                        return new BadRequestObjectResult("Error inserting user into the database: " + ex.Message);
                    }
                }
            }
        }
    }
}
