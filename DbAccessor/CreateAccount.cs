using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace GoRideShare
{
    public class CreateAccount(ILogger<CreateAccount> logger)
    {
        private readonly ILogger<CreateAccount> _logger = logger;

        [Function("CreateAccount")]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
        {
            UserInfo userToRegister = new(Guid.NewGuid(), "sample@email.com", 
                "cc3f4fd9608d575655ed31844b2349cf37be8ec5e4b0ec8ba9994fbc6653666f",
                "Bob Marley", "RIP", "Json list of preferences", "14312245323", 20, 4.8, "pictureEncoding");
            
            string json = JsonSerializer.Serialize<UserInfo>(userToRegister);

            _logger.LogInformation("C# HTTP trigger function processed a request.");
            return new ContentResult
            {
                Content = json,
                ContentType = "application/json",
                StatusCode = StatusCodes.Status200OK
            };
        }
    }
}
