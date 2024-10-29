using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Bson;
using Microsoft.Azure.Functions.Worker;

namespace GoRideShare
{
    public class GetAllConversations
    {

        private readonly string? _dbApiUrl;
        private readonly ILogger<GetAllConversations> _logger;
        // initialize the MongoDB client lazily. This is a best practice for serverless functions because it is not efficient to establish Mongo connections on every execution of our Azure Function
        public static Lazy<MongoClient> lazyClient = new Lazy<MongoClient>(InitializeMongoClient);
        public static MongoClient client = lazyClient.Value;

        public static MongoClient InitializeMongoClient()
        {
            return new MongoClient(Environment.GetEnvironmentVariable("MONGODB_ATLAS_URI"));
        }

        public GetAllConversations(ILogger<GetAllConversations> logger)
        {
            _logger = logger;
            _dbApiUrl = Environment.GetEnvironmentVariable("DB_URL");
        }

        [Function("GetAllConversations")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
        {
            // If validation result is not null, return the bad request result
            var validationResult = Utilities.ValidateHeaders(req.Headers, out Guid userId);
            if (validationResult != null)
            {
                return validationResult;
            }

            // Get the database collection and insert the new conversation
            IMongoCollection<Conversation> myConversations = client.GetDatabase("user_chats").GetCollection<Conversation>("conversations");

            // Get all conversations where the user string is included in the list of userIDs
            BsonDocument filter = new BsonDocument{ { "users", userId } };
            List<ConversationResponse> responseObj = new List<ConversationResponse>();

            try
            {
                // Fetch only the latest message of each conversation by checking its timestamp property
                var conversationsToFind = await myConversations.FindAsync(filter);

                // Fetch conversations from MongoDB
                List<Conversation> conversations = await conversationsToFind.ToListAsync();

                if(conversations.Count > 0)
                {
                    // Fetch users from SQL
                    List<Guid> userIds = [.. conversations.SelectMany(convo => convo.Users).Where(u => u != userId)];
                    List<User> users = await FetchUser.FetchUsers(userIds, _logger);    //Throws an Exception
                    Guid otherUser = Guid.Empty;
                    
                    //Connect the User info to their conversation
                    foreach (var convo in conversations)
                    {
                        otherUser = convo.Users.First(u => u != userId);
                        responseObj.Add(new ConversationResponse(convo.ConversationId, users.First(u => u.UserId == otherUser), convo.Messages));
                    }
                }
                return new OkObjectResult(responseObj);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "A DB error occurred while retrieving conversations.");
                return new ObjectResult("A DB error occurred while retrieving conversations") { StatusCode = StatusCodes.Status500InternalServerError };
            }
        }

    }

}
