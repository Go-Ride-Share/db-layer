using System.Text.Json.Serialization;

namespace GoRideShare.users
{
    public class PasswordLoginCredentials(string email, string passwordHash)
    {
        [JsonPropertyName("email")]
        public string Email { get; set; } = email;
        [JsonPropertyName("password")]
        public string PasswordHash { get; set; } = passwordHash;
    }

    public class GoogleLoginCredentials
    {
        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("password")]
        public string PasswordHash { get; set; } = "googleuser";

        [JsonPropertyName("id")]
        public string? UserId {get; set;}

        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }
    
    public class UserRegistrationInfo
    {
        [JsonPropertyName("user_id")]
        public string? UserId { get; set; }
        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("password")]
        public string? PasswordHash { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("bio")]
        public string? Bio { get; set; }

        [JsonPropertyName("phone")]
        public string? PhoneNumber { get; set; }

        [JsonPropertyName("photo")]
        public string? Photo { get; set; }
    }

}
