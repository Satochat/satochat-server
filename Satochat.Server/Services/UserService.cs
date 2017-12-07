using System.Linq;
using Satochat.Server.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Satochat.Server.Services {
    public class UserService : IUserService {
        private readonly SatochatContext _dbContext;

        public UserService(SatochatContext dbContext) {
            _dbContext = dbContext;
        }

        public async Task<User> FindUserByUuidAsync(string uuid) {
            return await _dbContext.Users.FirstOrDefaultAsync(e => e.Uuid == uuid);
        }

        public async Task<User> FindUserByUuidWithFriendsAsync(string uuid) {
            return await _dbContext.Users.Include(e => e.Friends).FirstOrDefaultAsync(e => e.Uuid == uuid);
        }

        public async Task<bool> UsersAreFriendsAsync(User[] users)  {
            if (users.Length == 0) {
                return false;
            }

            for (int i = 0; i < users.Length; ++i) {
                for (int j = 0; j < users.Length; ++j) {
                    User user1 = users[i];
                    User user2 = users[j];

                    if (user1.Id == user2.Id) {
                        continue;
                    }

                    bool friends = await _dbContext.UserFriends.AnyAsync(e => e.UserId == user1.Id && e.FriendId == user2.Id);
                    if (!friends) {
                        return false;
                    }
                }
            }
            
            return true;
        }
    }
}
