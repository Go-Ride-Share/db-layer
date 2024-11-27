using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Microsoft.Azure.Functions.Worker;

namespace GoRideShare.messages
{

    public class CreateConversation
    {
        private readonly ILogger<CreateConversation> _logger;

        // initialize the MongoDB client lazily. This is a best practice for serverless functions because it is not efficient to establish Mongo connections on every execution of our Azure Function
        public static Lazy<MongoClient> lazyClient = new Lazy<MongoClient>(InitializeMongoClient);
        public static MongoClient client = lazyClient.Value;

        public static MongoClient InitializeMongoClient()
        {
            return new MongoClient(Environment.GetEnvironmentVariable("MONGODB_ATLAS_URI"));
        }

        public CreateConversation(ILogger<CreateConversation> logger)
        {
            _logger = logger;
        }
        
        [Function("ConversationsPost")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route ="Conversations")] HttpRequest req)
        {
            // If validation result is not null, return the bad request result
            var validationResult = Utilities.ValidateHeaders(req.Headers, out string userId);
            if (validationResult != null)
            {
                _logger.LogError("Invalid Headers");
                return validationResult;
            }

            // Read the request body to get the user's registration information
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            _logger.LogInformation($"Raw Request Body: {JsonSerializer.Serialize(requestBody)}");

            ConversationRequest? convoRequest;
            try
            {
                convoRequest = JsonSerializer.Deserialize<ConversationRequest>(requestBody);

                if (convoRequest != null) {
                    var (invalid, errorMessage) = convoRequest.validate();
                    if (invalid)
                    {
                        _logger.LogError($"convoRequest is not valid: {errorMessage}");
                        return new BadRequestObjectResult(errorMessage);
                    }
                } else {
                    _logger.LogError("Input was null");
                    return new BadRequestObjectResult("Input was null");
                }
            }
            catch (JsonException ex)
            {
                _logger.LogError($"JSON deserialization failed: {ex.Message}");
                return new BadRequestObjectResult("Incomplete Conversation Request data.");
            }

            if (convoRequest.userId == userId)
            {
                _logger.LogError("You cannot start a conversation with yourself");
                return new BadRequestObjectResult("You cannot start a conversation with yourself"); 
            }

            User? recipient;
            try
            {
                recipient = await UserDB.FetchUser(convoRequest.userId);
                if ( recipient == null) {
                    _logger.LogError("Failed to get user details from DB");
                    return new ObjectResult("Failed to get user details from DB") { StatusCode = StatusCodes.Status500InternalServerError };
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"A Database Error Occured: {e.Message}");
                return new ObjectResult($"Failed to access the DB: {e.Message}") { StatusCode = StatusCodes.Status500InternalServerError };
            }

            Message newMessage = new Message
            (
                senderId: userId,
                contents: convoRequest.Contents, 
                timeStamp: convoRequest.TimeStamp
            );
            
            // Create a document for DB insertion
            var newConversation = new Conversation
            (
                new List<string> 
                {
                    convoRequest.userId,
                    userId
                }, 
                new List<Message> { newMessage }
            );

            // Get the database collection and insert the new conversation
            IMongoCollection<Conversation> convoCollection = client.GetDatabase("user_chats").GetCollection<Conversation>("conversations");

            try
            {
                await convoCollection.InsertOneAsync(newConversation);
                
                // Response object includes User details, hence its seperate from the conversation object
                var responseObj = new ConversationResponse
                (
                    newConversation.ConversationId, 
                    recipient, 
                    new List<Message> {newMessage} 
                );

                return new OkObjectResult(responseObj);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error: " + ex.Message);
                return new ObjectResult($"Failed to insert document: {ex.Message}") { StatusCode = StatusCodes.Status500InternalServerError };
            }
        }

    }

}
