using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;

namespace GoRideShare
{
    public static class Utilities
    {
        // Method that validates headers, outputs the userID and dbToken, returns exception if headers  missing, null if headers are good
        public static IActionResult? ValidateHeaders(IHeaderDictionary headers, out Guid userId)
        {
            userId = Guid.Empty;
            // Check for X-User-ID  and X-DbToken headers
            if (!headers.TryGetValue("X-User-ID", out var userIdValue) || string.IsNullOrWhiteSpace(userIdValue))
            {
                return new BadRequestObjectResult("Missing the following header: 'X-User-ID'.");
            }
            try
            {
                userId = Guid.Parse(userIdValue.ToString());
            }
            catch (FormatException)
            {
                return new BadRequestObjectResult("ERROR: Invalid X-User-ID Header: Not a Guid");
            }

            return null; // All headers are valid
        }

        public async static Task<(bool, string)> ValidateConnection(String? connectionString, MySqlConnection connection)
        {
            // Validate the connection string before trying to open the connection
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                return (true, "Invalid connection string.");
            }

            try
            {
                // open the connection with the database
                await connection.OpenAsync();
            }
            catch (MySqlException ex)
            {
                // Log the error and return an appropriate response
                return (true, $"Failed to open database connection: {ex.Message}");
            }
            return (false, "");
        }
    }
}
