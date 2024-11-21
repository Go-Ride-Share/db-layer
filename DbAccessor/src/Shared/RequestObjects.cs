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

        [JsonPropertyName("id")]
        public string? Userid { get; set; }

    }

    public class PostDetails
    {
        [JsonPropertyName("postId")]
        public Guid? PostId { get; set; }
        
        [JsonRequired]
        [JsonPropertyName("posterId")]
        public required string PosterId { get; set; }

        [JsonRequired]
        [JsonPropertyName("name")]
        public required string Name { get; set; }

        [JsonRequired]
        [JsonPropertyName("description")]
        public required string Description { get; set; }

        [JsonRequired]
        [JsonPropertyName("originLat")]
        public required float OriginLat { get; set; }

        [JsonRequired]
        [JsonPropertyName("originLng")]
        public required float OriginLng { get; set; }

        [JsonRequired]        
        [JsonPropertyName("destinationLat")]
        public required float DestinationLat { get; set; }

        [JsonRequired]
        [JsonPropertyName("destinationLng")]
        public required float DestinationLng { get; set; }

        [JsonRequired]
        [JsonPropertyName("price")]
        public required float Price { get; set; }

        [JsonRequired]
        [JsonPropertyName("seatsAvailable")]
        public required int SeatsAvailable { get; set; }

        [JsonRequired]
        [JsonPropertyName("departureDate")]
        public required string DepartureDate { get; set; }

        public PostDetails(){}

        public (bool, string) validate()
        {
            if (DepartureDate == "")
            {
                return (true, "DepartureDate cannot be empty");
            }
            if (Description == "")
            {
                return (true, "Description cannot be empty");
            }
            if (Name == "")
            {
                return (true, "Name cannot be empty");
            }
            if ( 90 < OriginLat || OriginLat < -90 )
            {
                return (true, "OriginLat is Invalid");
            }
            if ( 180 < OriginLng || OriginLng < -180 )
            {
                return (true, "OriginLat is Invalid");
            }
            if ( 180 < OriginLng || OriginLng < -180 )
            {
                return (true, "OriginLng is Invalid");
            }
            if ( 180 < DestinationLng || DestinationLng < -180 )
            {
                return (true, "DestinationLng is Invalid");
            }
            
            return (false, "");
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
        public string ConversationId { get; set; }

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
            List<string> users,
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
