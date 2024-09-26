using System.Configuration;

namespace GoRideShare
{
    public class LoginCredentials(string email, string passwordHash)
    {
        public string Email { get; set; } = email;
        public string PasswordHash { get; set; } = passwordHash;
    }

    public class UserInfo(Guid uid, string email, string passwordHash, string name,
                    string bio, string preferences, string phoneNumber, int numberOfRaitings,
                    double raitingAverage, string photo)
    {
        public Guid Uid { get; set; } = uid;
        public string Email { get; set; } = email;
        public string PasswordHash { get; set; } = passwordHash;
        public string Name { get; set; } = name;
        public string Bio { get; set; } = bio;
        public string Preferences { get; set; } = preferences;
        public string PhoneNumber { get; set; } = phoneNumber;
        public int NumberOfRaitings { get; set; } = numberOfRaitings;
        public double RaitingAverage { get; set; } = raitingAverage;
        public string Photo { get; set; } = photo;
    }

}