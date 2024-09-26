namespace GoRideShare
{
    public class LoginCredentials(string email, string passwordHash)
    {
        public string Email = email;
        public string PasswordHash = passwordHash;
    }
}