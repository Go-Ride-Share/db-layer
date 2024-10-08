using System.Configuration;

namespace GoRideShare
{
    public class LoginCredentials(string email, string passwordHash)
    {
        public string Email { get; set; } = email;
        public string PasswordHash { get; set; } = passwordHash;
    }

    public class UserRegistrationInfo(string email, string passwordHash, string name,
                    string bio, string preferences, string phoneNumber, string photo)
    {
        public string Email { get; set; } = email;
        public string PasswordHash { get; set; } = passwordHash;
        public string Name { get; set; } = name;
        public string Bio { get; set; } = bio;
        public string Preferences { get; set; } = preferences;
        public string PhoneNumber { get; set; } = phoneNumber;
        public string Photo { get; set; } = photo;
    }

}