using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace GoRideShare
{
    public static class Utilities
    {
        // Method that validates headers, outputs the userID and dbToken, returns exception if headers  missing, null if headers are good
        public static IActionResult? ValidateHeaders(IHeaderDictionary headers, out Guid userId)
        {
            userId = Guid.Empty;
            // Check for X-User-ID  and X-DbToken headers
            if (!headers.TryGetValue("X-User-ID", out var userIdValue) || string.IsNullOrWhiteSpace(userIdValue))
            {
                return new BadRequestObjectResult("Missing the following header: 'X-User-ID'.");
            }
            try
            {
                userId = Guid.Parse(userIdValue.ToString());
            }
            catch (FormatException)
            {
                return new BadRequestObjectResult("ERROR: Invalid X-User-ID Header: Not a Guid");
            }

            return null; // All headers are valid
        }

        internal static async Task<(bool error, string response)> MakeHttpGetRequest(Guid xUserId, string endpoint)
        {
            // Create a new HttpClient instance
            using (var client = new HttpClient())
            {
            // Create a new HttpRequestMessage instance
            using (var request = new HttpRequestMessage(HttpMethod.Get, endpoint))
            {
                // Add the X-User-ID header to the request
                request.Headers.Add("X-User-ID", xUserId.ToString());

                // Send the request and get the response
                var response = await client.SendAsync(request);

                // if its successful, then we know its 200, otherwise 404
                if (response.IsSuccessStatusCode) {
                    return (false, await response.Content.ReadAsStringAsync());
                } else {
                    return (true, response.StatusCode.ToString());
                }
            }
            }
        }
    }
}
