using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace GoRideShare
{
    public static class Utilities
    {
        // Method that validates headers, outputs the userID and dbToken, returns exception if headers  missing, null if headers are good
        public static IActionResult ValidateHeaders(IHeaderDictionary headers, out string userId)
        {
            userId = string.Empty;
            
            // Check for X-User-ID  and X-DbToken headers
            if (!headers.TryGetValue("X-User-ID", out var userIdValue) || string.IsNullOrWhiteSpace(userIdValue))
            {
                return new BadRequestObjectResult("Missing the following header: 'X-User-ID'.");
            }
            userId = userIdValue.ToString();

            return null; // All headers are valid
        }

        internal static async Task<(bool error, string response)> MakeHttpGetRequest(string xUserId, string endpoint)
        {
            // Create a new HttpClient instance
            using (var client = new HttpClient())
            {
            // Create a new HttpRequestMessage instance
            using (var request = new HttpRequestMessage(HttpMethod.Get, endpoint))
            {
                // Add the X-User-ID header to the request
                request.Headers.Add("X-User-ID", xUserId);

                // Send the request and get the response
                var response = await client.SendAsync(request);

                // if its successful, then we know its 200, otherwise 404
                return (response.IsSuccessStatusCode, await response.Content.ReadAsStringAsync());
            }
            }
        }
    }
}
