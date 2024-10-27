using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Bson;
using Microsoft.Azure.Functions.Worker;

namespace GoRideShare
{
    public class GetAllConversations(ILogger<GetAllConversations> logger)
    {

        private readonly ILogger<GetAllConversations> _logger = logger;
        // initialize the MongoDB client lazily. This is a best practice for serverless functions because it is not efficient to establish Mongo connections on every execution of our Azure Function
        public static Lazy<MongoClient> lazyClient = new Lazy<MongoClient>(InitializeMongoClient);
        public static MongoClient client = lazyClient.Value;

        public static MongoClient InitializeMongoClient()
        {
            return new MongoClient(Environment.GetEnvironmentVariable("MONGODB_ATLAS_URI"));
        }

        [Function("GetAllConversations")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
        {
            // If validation result is not null, return the bad request result
            var validationResult = Utilities.ValidateHeaders(req.Headers, out string userId);
            if (validationResult != null)
            {
                return validationResult;
            }

            // Get the database collection and insert the new conversation
            IMongoCollection<Conversation> myConversations = client.GetDatabase("user_chats").GetCollection<Conversation>("conversations");

            // Get all conversations where the user string is included in the list of userIDs
            BsonDocument filter = new BsonDocument{
                { "users", userId }
            };

            try
            {
                var conversationsToFind = await myConversations.FindAsync(filter);
                List<Conversation> conversations = await conversationsToFind.ToListAsync();
                // keep only latest message of each conversation by checking its timestamp property
                foreach (var convo in conversations)
                {
                    convo.Messages = convo.Messages.OrderByDescending(m => m.TimeStamp).Take(1).ToList();
                }
                return new OkObjectResult(conversations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "A DB error occurred while retrieving conversations.");
                return new ObjectResult("A DB error occurred while retrieving conversations") { StatusCode = StatusCodes.Status500InternalServerError };
            }
        }

    }

}
