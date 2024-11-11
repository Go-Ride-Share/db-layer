using System.Text.Json.Serialization;

namespace GoRideShare
{
    public class User
    {          
        [JsonPropertyName("userId")]
        public Guid? UserId { get; set; }

        [JsonRequired]
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("photo")]
        public string? Photo  { get; set; }

        public User(){}

        public User
        (
            Guid userId,
            string name,
            string? photo
        )
        {
            UserId = userId;
            Name = name;
            Photo = photo;
        }
        
        public (bool, string) validate()
        {
            if ( Name == "")
            {
                return (true, "name cannot be empty");
            }
            if ( Photo == "")
            {
                return (true, "photo cannot be empty");
            }
            return (false, "");
        }
    }
}
