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
            CreateConversationRequest? convoRequest;
            try
            {
                convoRequest = JsonSerializer.Deserialize<CreateConversationRequest>(requestBody);

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

            Message newMessage = new Message
            (
                convoRequest.UserId, 
                convoRequest.Contents, 
                convoRequest.TimeStamp
            );
            
            // Creates a new Document for the conversation
            var newConversation = new Conversation
            (
                new List<string> 
                {
                    convoRequest.UserId,
                    userId
                }, 
                new List<Message> { newMessage }
            );

            // Get the database collection and insert the new conversation
            IMongoCollection<Conversation> convoCollection = client.GetDatabase("user_chats").GetCollection<Conversation>("conversations");

            try
            {
                await convoCollection.InsertOneAsync(newConversation);
                var insertedId = newConversation.ConversationId;
                return new OkObjectResult($"Document inserted successfully with ID: {insertedId}");
            }
            catch (Exception ex)
            {
                return new ObjectResult($"Failed to insert document: {ex.Message}") { StatusCode = StatusCodes.Status500InternalServerError };
            }
        }

    }

}