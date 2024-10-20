using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Bson;
using Microsoft.Azure.Functions.Worker;

namespace GoRideShare
{

    public class PostMessage(ILogger<CreateConversation> logger)
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
            IncomingMessageRequest? messageReq = null;
            try
            {
                messageReq = JsonSerializer.Deserialize<IncomingMessageRequest>(requestBody);
                var (invalid, errorMessage) = messageReq.validate();
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


            // Get the database collection and insert the new conversation
            IMongoCollection<Conversation> convoToFind = client.GetDatabase("user_chats").GetCollection<Conversation>("conversation");

            // Find the conversation according to the user id in messageReq
            var convoRequest = convoToFind.Find<Conversation>(c => c.Users.Contains(messageReq.UserId)).FirstOrDefault();

            // add a new message to the conversation
            convoRequest.Messages.Add(new Message(messageReq.TimeStamp, messageReq.UserId, messageReq.Contents));

            // Update the conversation in the database
            var update = Builders<Conversation>.Update.Set("messages", convoRequest.Messages);

            // Gotta return the conversation with object id etc eventually
            var newConversation = convoRequest; // Assuming convoRequest is the updated conversation
            return new OkObjectResult(newConversation);
        }

    }

}