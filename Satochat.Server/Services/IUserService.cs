using Satochat.Server.Models;
using System.Threading.Tasks;

namespace Satochat.Server.Services {
    public interface IUserService {
        Task<User> FindUserByUuidAsync(string uuid);
        Task<User> FindUserByUuidWithFriendsAsync(string uuid);
        Task<bool> UsersAreFriendsAsync(User[] users);
    }
}
