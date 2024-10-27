using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Bson;
using Microsoft.Azure.Functions.Worker;

namespace GoRideShare
{
    public class PostMessage(ILogger<PostMessage> logger)
    {

        private readonly ILogger<PostMessage> _logger = logger;
        // initialize the MongoDB client lazily. This is a best practice for serverless functions because it is not efficient to establish Mongo connections on every execution of our Azure Function
        public static Lazy<MongoClient> lazyClient = new Lazy<MongoClient>(InitializeMongoClient);
        public static MongoClient client = lazyClient.Value;

        public static MongoClient InitializeMongoClient()
        {
            return new MongoClient(Environment.GetEnvironmentVariable("MONGODB_ATLAS_URI"));
        }

        [Function("PostMessage")]
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
            PostMessageRequest? incomingRequest;
            try
            {
                incomingRequest = JsonSerializer.Deserialize<PostMessageRequest>(requestBody);

                var (invalid, errorMessage) = incomingRequest.validate();
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

            // Create a new message object
            Message newMessage = new Message
            (
                incomingRequest.UserId, 
                incomingRequest.Contents, 
                incomingRequest.TimeStamp
            );

            _logger.LogInformation("conversationId: " + incomingRequest.ConversationId);
            // Get the database collection and insert the new conversation
            IMongoCollection<Conversation> myConversations = client.GetDatabase("user_chats").GetCollection<Conversation>("conversations");

            // // Get all conversations where the user string is included in the list of userIDs
            // BsonDocument filter = new BsonDocument{
            //     { "_id", new ObjectId(incomingRequest.ConversationId) }
            // };

            try
            {
                // var conversationToFind = await myConversations.FindAsync(filter);
                // Conversation conversation = await conversationToFind.FirstOrDefaultAsync();

                // Push the new message to the existing Messages array in the same conversation on db
                var filter = Builders<Conversation>.Filter.Eq("_id", new ObjectId(incomingRequest.ConversationId));
                var update = Builders<Conversation>.Update.Push("messages", newMessage);
                await myConversations.FindOneAndUpdateAsync(filter, update);
                // Console.WriteLine("Update Result: " + updateResult);
                // if (updateResult.ModifiedCount > 0)
                // {
                    return new OkObjectResult($"Message added successfully with ID: {incomingRequest.ConversationId}");
                // }
                // else
                // {
                //     return new ObjectResult("Message was not added.") { StatusCode = StatusCodes.Status500InternalServerError };
                // }
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