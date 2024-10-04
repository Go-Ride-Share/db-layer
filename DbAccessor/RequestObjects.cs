using System.Configuration;
using System.Text.Json.Serialization;

namespace GoRideShare
{
    public class LoginCredentials(string email, string passwordHash)
    {
        [JsonPropertyName("email")]
        public string Email { get; set; } = email;
        [JsonPropertyName("password")]
        public string PasswordHash { get; set; } = passwordHash;
    }

    public class UserRegistrationInfo(string email, string passwordHash, string name,
                    string bio, string phoneNumber, string photo)
    {
        [JsonPropertyName("email")]
        public string Email { get; set; } = email;

        [JsonPropertyName("password")]
        public string PasswordHash { get; set; } = passwordHash;

        [JsonPropertyName("name")]
        public string Name { get; set; } = name;

        [JsonPropertyName("bio")]
        public string Bio { get; set; } = bio;

        [JsonPropertyName("phone")]
        public string PhoneNumber { get; set; } = phoneNumber;

        [JsonPropertyName("photo")]
        public string Photo { get; set; } = photo;
    }
}