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

    public class IncomingConversationRequest
    {
        [JsonRequired]
        [JsonPropertyName("userId")]
        public string UserId  { get; set; }
        
        [JsonRequired]
        [JsonPropertyName("contents")]
        public string Contents { get; set; }

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


        public class Conversation
    {
        [JsonRequired]
        [JsonPropertyName("conversationId")]
        public string ConversationId { get; set; }

        [JsonRequired]
        [JsonPropertyName("messages")]
        public List<Message> Messages { get; set; }

        [JsonRequired]
        [JsonPropertyName("userID")]
        public string userID { get; set; }

        [JsonRequired]
        [JsonPropertyName("postId")]
        public string PostId { get; set; }

        public Conversation
        (
            string[] userIDs,
            List<Message> messages,
            string postId
        )
        {
            userID = userID;
            Messages = messages;
            PostId = postId;
        }
    }

    public class Message
    {
        [JsonRequired]
        [JsonPropertyName("timeStamp")]
        public DateTime TimeStamp  { get; set; }
        
        [JsonRequired]
        [JsonPropertyName("senderId")]
        public string SenderId { get; set; }

        [JsonRequired]
        [JsonPropertyName("contents")]
        public string Contents { get; set; }

        public Message
        (
            DateTime timeStamp,
            string senderId,
            string contents
        )
        {
            TimeStamp = timeStamp;
            SenderId = senderId;
            Contents = contents;
        }
    }

    [BsonIgnoreExtraElements]
    public class Movie
    {

        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("title")]
        [JsonRequired]
        [JsonPropertyName("title")]
        public string Title { get; set; } = null!;

        [BsonElement("plot")]
        [JsonRequired]
        [JsonPropertyName("plot")]
        public string Plot { get; set; } = null!;

        public Movie
        (
            string title,
            string plot
        )
        {
            Title = title;
            Plot = plot;
        }
    }

}
