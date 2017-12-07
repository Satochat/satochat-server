using System;
using System.Security.Cryptography;

namespace Satochat.Server.Models {
    // TODO: Link this with UserCredential instead of User
    public class AccessToken {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTimeOffset Expiry { get; set; } = DateTimeOffset.UtcNow.AddDays(1);
        public string Token { get; set; } = generateToken();
        public string IpAddress { get; set; }

        public virtual User User { get; set; }

        public AccessToken() {}

        public AccessToken(User user, string ipAddress) {
            User = user;
            IpAddress = ipAddress;
        }

        public bool IsExpired() => IsExpired(DateTimeOffset.UtcNow);

        public bool IsExpired(DateTimeOffset currentTime) => DateTimeOffset.UtcNow >= Expiry;

        private static string generateToken() {
            var rng = RNGCryptoServiceProvider.Create();
            byte[] tokenBytes = new byte[32];
            rng.GetBytes(tokenBytes);
            string token = Convert.ToBase64String(tokenBytes);
            return token;
        }
    }
}
