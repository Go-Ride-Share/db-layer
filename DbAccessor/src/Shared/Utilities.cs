using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System.Data.Common;

namespace GoRideShare
{
    public static class Utilities
    {
        // Method that validates headers, outputs the userID and dbToken, returns exception if headers  missing, null if headers are good
        public static IActionResult? ValidateHeaders(IHeaderDictionary headers, out string userId)
        {
            userId = "";
            // Check for X-User-ID  and X-DbToken headers
            if (!headers.TryGetValue("X-User-ID", out var userIdValue) || string.IsNullOrWhiteSpace(userIdValue))
            {
                return new BadRequestObjectResult("Missing the following header: 'X-User-ID'.");
            }
            userId = userIdValue.ToString();

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
   
        // Returns the user id from the reader. if an ordinal is given, gets it from that column ordinal,
        // If not, gets it from the column with given name. If not column name is given, gets it from column "user_id"
        public static string GetUserIdFromReader(DbDataReader reader, string columnName = "user_id", int ordinal = -1)
        {
            // if ordinal is -1, the user did not pass it. Need to use column name
            if (ordinal == -1)
                ordinal = reader.GetOrdinal(columnName);

            // Connector/Net 6.1.1 and later automatically treats char(36) as a Guid type, 
            // unless it does not follow a GUID pattern, then treats it as a string
            if(reader[ordinal].GetType() == typeof(Guid))
                return reader.GetGuid(ordinal).ToString();
            else
                return reader.GetString(ordinal);
        }
    }
}
