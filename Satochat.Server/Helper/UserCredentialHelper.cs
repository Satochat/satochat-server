using System;
using System.Security.Cryptography;
using System.Text;

namespace Satochat.Server.Helper {
    public static class UserCredentialHelper {

        public static string GenerateSalt() {
            var rng = RNGCryptoServiceProvider.Create();
            byte[] salt = new byte[32];
            rng.GetBytes(salt);
            return Convert.ToBase64String(salt);
        }

        public static string HashPassword(string password) {
            string salt = GenerateSalt();
            string digest = HashPassword(salt, password);
            return joinSaltAndHash(salt, digest);
        }

        public static string HashPassword(string salt, string password) {
            SHA256 sha = SHA256.Create();
            string joined = joinSaltAndPassword(salt, password);
            byte[] digest = sha.ComputeHash(Encoding.UTF8.GetBytes(joined));
            return Convert.ToBase64String(digest);
        }

        private static string joinSaltAndPassword(string salt, string password) {
            return salt + password;
        }

        private static string joinSaltAndHash(string salt, string hash) {
            return salt + "." + hash;
        }

        public static string[] SplitSaltAndHash(string raw) {
            return raw.Split(".", 2);
        }
    }
}
