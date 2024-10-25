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

}