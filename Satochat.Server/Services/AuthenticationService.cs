using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Satochat.Server.Models;
using Satochat.Server.Helper;
using Microsoft.Extensions.Logging;
using System.Net;
using Microsoft.EntityFrameworkCore;

namespace Satochat.Server.Services
{
    public class AuthenticationService : IAuthenticationService {

        private readonly ILogger<AuthenticationService> _logger;

        private readonly SatochatContext _dbContext;

        public AuthenticationService(ILogger<AuthenticationService> logger, SatochatContext dbContext) {
            _logger = logger;
            _dbContext = dbContext;
        }

        public async Task<User> AuthenticateUserAsync(string username, string password) {
            UserCredential potentialCredential = await _dbContext.UserCredentials.Include(e => e.User).SingleOrDefaultAsync(e => e.Username == username);
            if (potentialCredential == null) {
                // No such user
                return null;
            }

            string[] storedSaltAndHash = UserCredentialHelper.SplitSaltAndHash(potentialCredential.PasswordDigest);
            if (storedSaltAndHash.Length != 2) {
                // Log internal error
                return null;
            }

            string storedSalt = storedSaltAndHash[0];
            string storedHash = storedSaltAndHash[1];

            string hash = UserCredentialHelper.HashPassword(storedSalt, password);
            if (hash != storedHash) {
                // Password mismatch
                return null;
            }

            return potentialCredential.User;
        }

        public async Task<AccessToken> GetTokenAsync(User user, IPAddress ipAddress) {
            var accessToken = await _dbContext.AccessTokens.Where(e => e.UserId == user.Id && e.IpAddress == ipAddress.ToString()).OrderByDescending(e => e.Expiry).FirstOrDefaultAsync();

            // Try to reuse existing token
            if (accessToken != null) {
                if (!accessToken.IsExpired()) {
                    // Reuse existing token
                    return accessToken;
                }

                // Delete existing token
                //_dbContext.Remove(accessToken);
            }

            // Create new token
            accessToken = new AccessToken(user, ipAddress.ToString());
            user.AccessTokens.Add(accessToken);
            await _dbContext.SaveChangesAsync();
            return accessToken;
        }

        public Task<AccessToken> GetTokenAsync(string token) {
            return _dbContext.AccessTokens.Include(e => e.User).SingleOrDefaultAsync(e => e.Token == token);
        }

        public bool VerifyToken(AccessToken accessToken, IPAddress ipAddress) {
            if (accessToken == null) {
                return false;
            }

            if (ipAddress == null) {
                return false;
            }

            if (accessToken.IsExpired()) {
                return false;
            }

            IPAddress tokenIpAddress;
            if (!IPAddress.TryParse(accessToken.IpAddress, out tokenIpAddress)) {
                return false;
            }

            if (!ipAddress.Equals(tokenIpAddress)) {
                return false;
            }

            return true;
        }
    }
}
