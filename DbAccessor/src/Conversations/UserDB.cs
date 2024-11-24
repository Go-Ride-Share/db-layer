using MySql.Data.MySqlClient;

namespace GoRideShare.messages
{
    public static class UserDB
    {

        public static async Task<List<User>> FetchUsers(List<string> users)
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
                            bool hasNulls = reader.IsDBNull(0) || reader.IsDBNull(1);

                            if(!hasNulls)
                            {
                                string user_id = Utilities.GetUserIdFromReader(reader);
                                var name = reader.GetString(1);
                                var photo = reader.IsDBNull(2) ? null : reader.GetString(2);
                                userObjects.Add( new User(user_id, name, photo ) );
                            }
                        }
                        return userObjects;
                    }
                }
            }
        }

        public static async Task<User?> FetchUser(string userId)
        {
            // Retrieve the database connection string from environment variables
            string? connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");
            using (var connection = new MySqlConnection(connectionString))
            {
                // Open the connection with the database
                await connection.OpenAsync(); // Throws an exceptions
                
                // Query to retrieve user by userID
                var query = $"SELECT user_id, name, photo FROM users WHERE user_id IN ( '{userId}' )";
                using (var command = new MySqlCommand(query, connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            //Double check all of the required fields are present
                            bool hasNulls = reader.IsDBNull(0) || reader.IsDBNull(1);

                            if(!hasNulls)
                            {
                                string user_id = Utilities.GetUserIdFromReader(reader);
                                var name = reader.GetString(1);
                                var photo = reader.IsDBNull(2) ? "" : reader.GetString(2);
                                return new User(user_id, name, photo );
                            }
                            return null;
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
            }
        }


    }
}