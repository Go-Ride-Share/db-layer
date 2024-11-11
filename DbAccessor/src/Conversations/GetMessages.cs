using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Bson;
using Microsoft.Azure.Functions.Worker;

namespace GoRideShare.messages
{
    public class GetMessages
    {

        private readonly ILogger<GetMessages> _logger;

        // initialize the MongoDB client lazily. This is a best practice for serverless functions because it is not efficient to establish Mongo connections on every execution of our Azure Function
        public static Lazy<MongoClient> lazyClient = new Lazy<MongoClient>(InitializeMongoClient);
        public static MongoClient client = lazyClient.Value;

        public static MongoClient InitializeMongoClient()
        {
            return new MongoClient(Environment.GetEnvironmentVariable("MONGODB_ATLAS_URI"));
        }

        public GetMessages(ILogger<GetMessages> logger)
        {
            _logger = logger;
        }

        [Function("MessagesGet")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route ="Messages/{conversation_id}")] HttpRequest req, string conversation_id)
        {
            // If validation result is not null, return the bad request result
            var validationResult = Utilities.ValidateHeaders(req.Headers, out Guid userId);
            if (validationResult != null)
            {
                _logger.LogError("Invalid Headers");
                return validationResult;
            }
            
            // Timestamp is an optional parameter to limit the response size
            DateTime? dateTimeLimit = null;
            if (req.Query.TryGetValue("timeStamp", out var timeStamp))
            {
                // convert the timeStamp into a datetime object
                if (!DateTime.TryParse(timeStamp, out DateTime parsedDateTime))
                {
                    return new BadRequestObjectResult("Invalid timestamp format. Please provide a valid timestamp in the format: yyyy-MM-ddTHH:mm:ss");
                }
                dateTimeLimit = parsedDateTime;
            }

            // Limit is an optional Query paramter to limit the number of messages returned
            int pollingLimit = 50; //we set default limit to 50
            
            if (req.Query.TryGetValue("limit", out var limitStr))
            {
                if (!int.TryParse(limitStr, out int parsedLimit))
                {
                    return new BadRequestObjectResult("Invalid limit format. Please provide a valid integer value for the limit parameter");
                }
                pollingLimit = parsedLimit;
            }

            // Get the database collection and insert the new conversation
            IMongoCollection<Conversation> myConversations = client.GetDatabase("user_chats").GetCollection<Conversation>("conversations");

            // Get all conversations where the user string is included in the list of userIDs
            BsonDocument filter = new BsonDocument{
                { "_id", new ObjectId(conversation_id) }
            };

            try
            {
                var conversationToFind = await myConversations.FindAsync(filter);
                Conversation conversation = await conversationToFind.FirstOrDefaultAsync();
                // filter first 50 messages based on the timestamp. And sort it from latest to oldest
                conversation.Messages = conversation.Messages.Where(m => dateTimeLimit == null || m.TimeStamp > dateTimeLimit).OrderByDescending(m => m.TimeStamp).Take(pollingLimit).ToList();

                // fetch the other person's userId from the conversation object
                Guid otherUserId = conversation.Users.First(u => u != userId);
                User? otherUser;

                try
                {
                    otherUser = await UserDB.FetchUser(otherUserId);
                    if ( otherUser == null) {
                        return new ObjectResult($"ERROR: Failed to get user details from DB") { StatusCode = StatusCodes.Status500InternalServerError };
                    }
                }
                catch (Exception e)
                {
                    return new ObjectResult($"ERROR: Failed to access the DB: {e.Message}") { StatusCode = StatusCodes.Status500InternalServerError };
                }

                // create a response object
                var responseObj = new ConversationResponse
                (
                    conversation_id, 
                    otherUser, 
                    conversation.Messages
                );

                return new OkObjectResult(responseObj);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "A DB error occurred while retrieving conversations.");
                // return a 500 error and include the error message in the body too
                return new ObjectResult($"Failed to retrieve conversations: {ex.Message}") { StatusCode = StatusCodes.Status500InternalServerError };
                
            }
        }

    }

}
