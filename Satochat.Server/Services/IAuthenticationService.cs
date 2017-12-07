using Satochat.Server.Models;
using System.Net;
using System.Threading.Tasks;

namespace Satochat.Server.Services {
    public interface IAuthenticationService {
        Task<User> AuthenticateUserAsync(string username, string password);
        Task<AccessToken> GetTokenAsync(User user, IPAddress ipAddress);
        Task<AccessToken> GetTokenAsync(string token);
        bool VerifyToken(AccessToken accessToken, IPAddress ipAddress);
    }
}
