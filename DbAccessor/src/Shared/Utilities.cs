using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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
    }
}