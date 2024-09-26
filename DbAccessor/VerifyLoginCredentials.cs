using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace GoRideShare
{
    public class VerifyLoginCredentials(ILogger<VerifyLoginCredentials> logger)
    {
        private readonly ILogger<VerifyLoginCredentials> _logger = logger;

        [Function("VerifyLoginCredentials")]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
        {
            LoginCredentials loginCredentials = new("some email", "cc3f4fd9608d575655ed31844b2349cf37be8ec5e4b0ec8ba9994fbc6653666f");
            string json = JsonSerializer.Serialize<LoginCredentials>(loginCredentials);

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
