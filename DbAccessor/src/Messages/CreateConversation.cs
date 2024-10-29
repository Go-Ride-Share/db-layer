using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Bson;
using Microsoft.Azure.Functions.Worker;

namespace GoRideShare
{

    public class CreateConversation
    {
        private readonly ILogger<CreateConversation> _logger;
        private readonly string? _dbApiUrl;
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
            _dbApiUrl = Environment.GetEnvironmentVariable("DB_URL");
        }
        
        [Function("CreateConversation")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req)
        {
            // If validation result is not null, return the bad request result
            var validationResult = Utilities.ValidateHeaders(req.Headers, out Guid userId);
            if (validationResult != null)
            {
                return validationResult;
            }

            // Read the request body to get the user's registration information
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            ConversationRequest? convoRequest;
            try
            {
                convoRequest = JsonSerializer.Deserialize<ConversationRequest>(requestBody);

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


            // Get the user name and photo details by calling SQL Get user endpoint
            User otherUser;
            string endpoint = $"{_dbApiUrl}/api/GetUser";

            // make http request using req.Headers and endpoint to get the user details
            var (error, response) = await Utilities.MakeHttpGetRequest(convoRequest.UserId, endpoint);
            if (!error && response != null)
            {
                _logger.LogInformation("Response: " + response);
                otherUser = JsonSerializer.Deserialize<User>(response);
                otherUser.UserId = convoRequest.UserId;
            }else{
                _logger.LogError("Error" + response);
                return new ObjectResult($"Failed to get user details from DB: {response}") { StatusCode = StatusCodes.Status404NotFound };
            }

            Message newMessage = new Message
            (
                userId, // the user who initiated the conversation by sending a message is the "SenderId" of the message
                convoRequest.Contents, 
                convoRequest.TimeStamp
            );
            
            // Create a document for DB insertion
            var newConversation = new Conversation
            (
                new List<Guid> 
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
                
                            // Response object includes User details, hence its seperate from the conversation object
                var responseObj = new ConversationResponse
                (
                    newConversation.ConversationId, 
                    otherUser, 
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
