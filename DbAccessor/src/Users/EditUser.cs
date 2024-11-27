using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using System.Text.Json;

namespace GoRideShare.users
{
    public class EditUser(ILogger<EditUser> logger)
    {
        private readonly ILogger<EditUser> _logger = logger;

        [Function("UserEdit")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "Users")] HttpRequest req)
        {
            // If validation result is not null, return the bad request result
            var validationResult = Utilities.ValidateHeaders(req.Headers, out string userId);
            if (validationResult != null)
            {
                _logger.LogError("Invalid Headers");
                return validationResult;
            }

            // Retrieve the database connection string from environment variables
            string? connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                _logger.LogError("Invalid connection string.");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }

            // Read and deserialize the request body
            UserRegistrationInfo? updatedUserInfo;
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                updatedUserInfo = JsonSerializer.Deserialize<UserRegistrationInfo>(requestBody);
                if (updatedUserInfo == null)
                {
                    return new BadRequestObjectResult("Invalid or missing request body.");
                }
            }
            catch (JsonException ex)
            {
                _logger.LogError($"JSON deserialization error: {ex.Message}");
                return new BadRequestObjectResult("Invalid JSON format.");
            }

            using (var connection = new MySqlConnection(connectionString))
            {
                try
                {
                    await connection.OpenAsync();
                }
                catch (MySqlException ex)
                {
                    _logger.LogError($"Failed to open database connection: {ex.Message}");
                    return new StatusCodeResult(StatusCodes.Status500InternalServerError);
                }

                // Build the query dynamically based on which fields the user wants to update
                var queryBuilder = new List<string>();
                var parameters = new List<MySqlParameter>();

                if (!string.IsNullOrWhiteSpace(updatedUserInfo.Bio))
                {
                    queryBuilder.Add("bio = @Bio");
                    parameters.Add(new MySqlParameter("@Bio", updatedUserInfo.Bio));
                }
                if (!string.IsNullOrWhiteSpace(updatedUserInfo.Name))
                {
                    queryBuilder.Add("name = @Name");
                    parameters.Add(new MySqlParameter("@Name", updatedUserInfo.Name));
                }
                if (!string.IsNullOrWhiteSpace(updatedUserInfo.PhoneNumber))
                {
                    queryBuilder.Add("phone_number = @PhoneNumber");
                    parameters.Add(new MySqlParameter("@PhoneNumber", updatedUserInfo.PhoneNumber));
                }
                if (!string.IsNullOrWhiteSpace(updatedUserInfo.Photo))
                {
                    queryBuilder.Add("photo = @Photo");
                    parameters.Add(new MySqlParameter("@Photo", updatedUserInfo.Photo));
                }

                if (queryBuilder.Count == 0)
                {
                    return new BadRequestObjectResult("No fields to update.");
                }

                string updateQuery = $"UPDATE users SET {string.Join(", ", queryBuilder)} WHERE user_id = @UserId";
                parameters.Add(new MySqlParameter("@UserId", userId.ToString()));

                using (var command = new MySqlCommand(updateQuery, connection))
                {
                    command.Parameters.AddRange(parameters.ToArray());

                    try
                    {
                        var rowsAffected = await command.ExecuteNonQueryAsync();
                        if (rowsAffected > 0)
                        {
                            return new OkResult(); // Return 200 OK if the update succeeded
                        }
                        else
                        {
                            return new NotFoundResult(); // User not found
                        }
                    }
                    catch (MySqlException ex)
                    {
                        _logger.LogError($"Database error: {ex.Message}");
                        return new BadRequestObjectResult("Error updating the database.");
                    }
                }
            }
        }
    }
}
