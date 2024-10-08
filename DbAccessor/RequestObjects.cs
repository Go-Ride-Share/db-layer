using System.Configuration;
using System.Text.Json.Serialization;

namespace GoRideShare
{
    public class LoginCredentials(string email, string passwordHash)
    {
        public string Email { get; set; } = email;
        public string PasswordHash { get; set; } = passwordHash;
    }

    public class UserRegistrationInfo(
        string email, 
        string passwordHash, 
        string name,
        string bio, 
        string phoneNumber, 
        string photo, 
        string preferences
        )

    {
        public string Email { get; set; } = email;
        public string PasswordHash { get; set; } = passwordHash;
        public string Name { get; set; } = name;
        public string Bio { get; set; } = bio;
        public string Preferences { get; set; } = preferences;
        public string PhoneNumber { get; set; } = phoneNumber;
        public string Photo { get; set; } = photo;
    }

    public class PostDetails
    {
        [JsonPropertyName("postId")]
        public string PostId { get; set; }
        
        [JsonPropertyName("posterId")]
        public string PosterId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("originLat")]
        public float OriginLat { get; set; }

        [JsonPropertyName("originLng")]
        public float OriginLng { get; set; }

        [JsonPropertyName("destinationLat")]
        public float DestinationLat { get; set; }

        [JsonPropertyName("destinationLng")]
        public float DestinationLng { get; set; }

        [JsonPropertyName("price")]
        public float Price { get; set; }

        [JsonPropertyName("seatsAvailable")]
        public int SeatsAvailable { get; set; }

        [JsonPropertyName("departureDate")]
        public required string DepartureDate { get; set; }

        public PostDetails() { }

        public PostDetails(
            string postId,
            string posterId,
            string name,
            string description,
            string departureDate,
            float originLat,
            float originLng,
            float destinationLat,
            float destinationLng,
            float price,
            int seatsAvailable)
        {
            PostId = postId;
            PosterId = posterId;
            Name = name;
            Description = description;
            OriginLat = originLat;
            OriginLng = originLng;
            DestinationLat = destinationLat;
            DestinationLng = destinationLng;
            Price = price;
            SeatsAvailable = seatsAvailable;
            DepartureDate = departureDate;
        }
    }
}