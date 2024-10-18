using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Bson;
using Microsoft.Azure.Functions.Worker;

namespace GoRideShare
{

    public class CreateConversation(ILogger<CreateConversation> logger)
    {

        private readonly ILogger<CreateConversation> _logger = logger;
        // initialize the MongoDB client lazily. This is a best practice for serverless functions because it is not efficient to establish Mongo connections on every execution of our Azure Function
        public static Lazy<MongoClient> lazyClient = new Lazy<MongoClient>(InitializeMongoClient);
        public static MongoClient client = lazyClient.Value;

        public static MongoClient InitializeMongoClient()
        {
            return new MongoClient(Environment.GetEnvironmentVariable("MONGODB_ATLAS_URI"));
        }


        [Function("CreateConversation")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req)
        {
            // If validation result is not null, return the bad request result
            var validationResult = Utilities.ValidateHeaders(req.Headers, out string userId);
            if (validationResult != null)
            {
                return validationResult;
            }

            // Read the request body to get the user's registration information
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            IncomingConversationRequest? convoRequest = null;
            try
            {
                convoRequest = JsonSerializer.Deserialize<IncomingConversationRequest>(requestBody);
                var (invalid, errorMessage) = convoRequest.validate();
                if (invalid)
                {
                    return new BadRequestObjectResult(errorMessage);
                }
            }
            catch (JsonException ex)
            {
                _logger.LogError($"JSON deserialization failed: {ex.Message}");
                return new BadRequestObjectResult("Incomplete Conversation Request data.");
            }
            _logger.LogInformation($"Raw Request Body: {JsonSerializer.Serialize(requestBody)}");


            // serialize the messages first
            Message? newMessage = null;
            try
            {
                newMessage = JsonSerializer.Deserialize<Message>(convoRequest.Contents);
            }
            catch (JsonException ex)
            {
                _logger.LogError($"JSON deserialization failed: {ex.Message}");
                return new BadRequestObjectResult("Invalid format or Incomplete Message data.");
            }
            
            // Creates a new Document for the conversation
            BsonDocument newConversation = new BsonDocument
            {
                { "users", new BsonArray { convoRequest.UserId, new BsonString(req.Headers["X-User-Id"].ToString()) } },
                { "messages", new BsonArray { new BsonDocument
                    {
                        { "senderId", newMessage.SenderId },
                        { "content", newMessage.Contents },
                        { "timestamp", newMessage.TimeStamp }
                    }
                }}
            };


            // Get the database collection and insert the new conversation
            IMongoCollection<BsonDocument> convoCollection = client.GetDatabase("user_chats").GetCollection<BsonDocument>("conversation");


            // Insert the new conversation
            await convoCollection.InsertOneAsync(newConversation);

            // Gotta return the conversation with object id etc eventually
            return new OkObjectResult(newConversation);
        }

    }

}