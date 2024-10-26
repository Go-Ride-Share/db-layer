using System.Configuration;
using System.Text.Json.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace MongoExample
{
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

    [BsonIgnoreExtraElements]
    public class Message
    {
        [BsonElement("senderId")]
        [JsonRequired]
        [JsonPropertyName("senderId")]
        public string SenderId { get; set; } = null!;

        [BsonElement("contents")]
        [JsonRequired]
        [JsonPropertyName("contents")]
        public string Contents { get; set; } = null!;

        [BsonElement("timeStamp")]
        [JsonRequired]
        [JsonPropertyName("timeStamp")]
        public BsonTimestamp TimeStamp { get; set; }
        public Message
        (
            string sender,
            string contents,
            BsonTimestamp timeStamp
        )
        {
            SenderId = sender;
            Contents = contents;
            TimeStamp = timeStamp;
        }
    }
}