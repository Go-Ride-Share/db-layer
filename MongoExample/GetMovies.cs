using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Bson;
using Microsoft.Azure.Functions.Worker;

namespace MongoExample
{

    public static class GetMovies
    {

        public static Lazy<MongoClient> lazyClient = new Lazy<MongoClient>(InitializeMongoClient);
        public static MongoClient client = lazyClient.Value;

        public static MongoClient InitializeMongoClient()
        {
            return new MongoClient(Environment.GetEnvironmentVariable("MONGODB_ATLAS_URI"));
        }

        [Function("GetMovies")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req) {
            IMongoCollection<Conversation> moviesCollection = client.GetDatabase("user_chats").GetCollection<Conversation>("conversations");

            var document = new Conversation
            (
                new List<string> 
                {
                    "user1",
                    "user2"
                }, 
                new List<Message> { new Message("user1", "Hello again with proper timestamp once again!",  BsonTimestamp.Create(DateTimeOffset.Now.ToUnixTimeSeconds())) }
            );


            try
            {
                await moviesCollection.InsertOneAsync(document);
                var insertedId = document.ConversationId;
                return new OkObjectResult($"Document inserted successfully with ID: {insertedId}");
                return new OkObjectResult("Document inserted successfully.");
            }
            catch (Exception ex)
            {
                return new ObjectResult($"Failed to insert document: {ex.Message}") { StatusCode = StatusCodes.Status500InternalServerError };
            }
        }
    }

}