using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;

namespace GoRideShare
{
    public static class UserDB
    {

        public static async Task<List<User>> FetchUsers(List<Guid> users)
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
                using (var command = new MySqlCommand(query, connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        var userObjects = new List<User>();
                        while (await reader.ReadAsync())
                        {
                            //Double check all of the required fields are present
                            bool hasNulls = typeof(PostDetails).GetProperties()
                                .Any(property => reader.IsDBNull(reader.GetOrdinal(property.Name)));

                            if(!hasNulls)
                            {
                                var user_id = reader.GetGuid(0);
                                var name = reader.GetString(1);
                                var photo = reader.GetString(2);
                                userObjects.Add( new User(user_id, name, photo ) );
                            }
                        }
                        return userObjects;
                    }
                }
            }
        }

        public static async Task<User?> FetchUser(Guid userId)
        {
            // Retrieve the database connection string from environment variables
            string? connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");
            using (var connection = new MySqlConnection(connectionString))
            {
                // Open the connection with the database
                await connection.OpenAsync(); // Throws an exceptions
                
                // Query to retrieve user by userID
                var query = $"SELECT user_id, name, photo FROM users WHERE user_id IN ( {userId} )";
                using (var command = new MySqlCommand(query, connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            //Double check all of the required fields are present
                            bool hasNulls = typeof(PostDetails).GetProperties()
                                .Any(property => reader.IsDBNull(reader.GetOrdinal(property.Name)));

                            if(!hasNulls)
                            {
                                var user_id = reader.GetGuid(0);
                                var name = reader.GetString(1);
                                var photo = reader.GetString(2);
                                return new User(user_id, name, photo );
                            }
                            return null;
                        }
                        else
                        {
                            // Return 404 Not Found if no user is found
                            return null;
                        }
                    }
                }
            }
        }


    }
}