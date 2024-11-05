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
    }
}
