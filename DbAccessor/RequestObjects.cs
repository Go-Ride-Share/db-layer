using System.Configuration;
using System.Text.Json.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace GoRideShare
{
    public class LoginCredentials(string email, string passwordHash)
    {
        [JsonPropertyName("email")]
        public string Email { get; set; } = email;
        [JsonPropertyName("password")]
        public string PasswordHash { get; set; } = passwordHash;
    }

    public class UserRegistrationInfo
    {
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

    public class PostMessageRequest
    {
        [JsonRequired]
        [JsonPropertyName("conversationId")]
        public required string ConversationId { get; set; }

        [JsonRequired]
        [JsonPropertyName("userId")]
        public required string UserId { get; set; }
        
        [JsonRequired]
        [JsonPropertyName("contents")]
        public required string Contents { get; set; }

        [JsonRequired]
        [JsonPropertyName("timeStamp")]
        public DateTime TimeStamp { get; set; }

        public (bool, string) validate()
        {
            if(ConversationId == "")
            {
                return (true, "conversationId is invalid");
            }
            if (Contents == "")
            {
                return (true, "contents cannot be empty");
            }
            if (UserId == "")
            {
                return (true, "userId is invalid");
            }
            if (TimeStamp == DateTime.MinValue || TimeStamp > DateTime.Now) // Check if the timestamp is in the future or invalid
            {
                return (true, "timeStamp is invalid");
            }
            return (false, "");
        }
    }

    public class ConversationRequest
    {
        [JsonRequired]
        [JsonPropertyName("userId")]
        public required string UserId  { get; set; }
        
        [JsonRequired]
        [JsonPropertyName("contents")]
        public required string Contents { get; set; }

        [JsonRequired]
        [JsonPropertyName("timeStamp")]
        public System.DateTime TimeStamp { get; set; }

        public (bool, string) validate()
        {
            if ( Contents == "")
            {
                return (true, "contents cannot be empty");
            }
            if ( UserId == "")
            {
                return (true, "userId is invalid");
            }
            if (TimeStamp == DateTime.MinValue || TimeStamp > DateTime.Now) // Check if the timestamp is in the future or invalid
            {
                return (true, "timeStamp is invalid");
            }
            return (false, "");
        }
    }

    public class ConversationResponse
    {
        [JsonRequired]
        [JsonPropertyName("conversationId")]
        public string ConversationId  { get; set; }

        [JsonRequired]
        [JsonPropertyName("user")]
        public User User  { get; set; }
        
        [JsonRequired]
        [JsonPropertyName("messages")]
        public List<Message> Messages { get; set; }

        public ConversationResponse
        (
            string conversationId, 
            User user,
            List<Message> messages
        )
        {
            ConversationId = conversationId;
            User = user;
            Messages = messages;
        }
    }

    [BsonIgnoreExtraElements]
    public class Conversation
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? ConversationId { get; set; }

        [BsonElement("users")]
        [JsonRequired]
        [JsonPropertyName("users")]
        public List<string> Users { get; set; } = null!;

        [BsonElement("messages")]
        [JsonRequired]
        [JsonPropertyName("messages")]
        public List<Message> Messages { get; set; } = null!;

        public Conversation
        (
            List<String> users,
            List<Message> messages
        )
        {
            Users = users;
            Messages = messages;
        }
    }

    public class Message
    {          
        [BsonElement("senderId")]
        [JsonRequired]
        [JsonPropertyName("senderId")]
        public string SenderId { get; set; }

        [BsonElement("contents")]
        [JsonRequired]
        [JsonPropertyName("contents")]
        public string Contents { get; set; }

        [BsonElement("timeStamp")]
        [JsonRequired]
        [JsonPropertyName("timeStamp")]
        public DateTime TimeStamp  { get; set; }

        public Message
        (
            string senderId,
            string contents,
            DateTime timeStamp
        )
        {
            TimeStamp = timeStamp;
            SenderId = senderId;
            Contents = contents;
        }
    }

        public class User
    {          
        [JsonPropertyName("userId")]
        public string? UserId { get; set; }

        [JsonRequired]
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonRequired]
        [JsonPropertyName("photo")]
        public string Photo  { get; set; }

        public User
        (
            string userId,
            string name,
            string photo
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
