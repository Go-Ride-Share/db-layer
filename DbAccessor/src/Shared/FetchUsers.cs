using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;

namespace GoRideShare
{
    public static class FetchUser
    {

        public static async Task<List<User>> FetchUsers(List<Guid> users, ILogger<GetAllConversations> _logger)
        {
            // Retrieve the database connection string from environment variables
            string? connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");
            using (var connection = new MySqlConnection(connectionString))
            {
                // Open the connection with the database
                await connection.OpenAsync(); // Throws an exceptions
                
                // Query to retrieve users by user ID
                string userList = string.Join(", ", users.ConvertAll(user => $"'{user}'"));
                var query = $"SELECT user_id, name, photo FROM users WHERE user_id IN ( {userList} )";
                _logger.LogWarning(userList);
                _logger.LogWarning(query);
                using (var command = new MySqlCommand(query, connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        var userObjects = new List<User>();
                        while (await reader.ReadAsync())
                        {
                            var user_id = reader.IsDBNull(0) ? Guid.Empty : reader.GetGuid(0);
                            var name = reader.IsDBNull(1) ? null : reader.GetString(1);
                            var photo = reader.IsDBNull(2) ? null : reader.GetString(2);
                            userObjects.Add( new User(user_id, name, photo ) );
                        }
                        return userObjects;
                    }
                }
            }
        }

    }
}