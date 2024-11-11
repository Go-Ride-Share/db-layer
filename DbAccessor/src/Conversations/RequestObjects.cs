using System.Text.Json.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace GoRideShare.messages
{
    public class MessageRequest
    {
        [JsonRequired]
        [JsonPropertyName("conversationId")]
        public required string ConversationId { get; set; }

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
            if (Contents.Length > 500)
            {
                return (true, "Message Contents is too large");
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
        [JsonPropertyName("recipientId")]
        public required Guid RecipientId  { get; set; }
        
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
        public List<Guid> Users { get; set; } = null!;

        [BsonElement("messages")]
        [JsonRequired]
        [JsonPropertyName("messages")]
        public List<Message> Messages { get; set; } = null!;

        public Conversation
        (
            List<Guid> users,
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
        public Guid SenderId { get; set; }

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
            Guid senderId,
            string contents,
            DateTime timeStamp
        )
        {
            TimeStamp = timeStamp;
            SenderId = senderId;
            Contents = contents;
        }
    }

}
