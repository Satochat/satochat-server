using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Satochat.Server.Models {
    public class UserFriend {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int FriendId { get; set; }
        // Allow communication from friend to user
        public bool AllowCommunication { get; set; }

        public virtual User User { get; set; }
        public virtual User Friend { get; set; }

        public UserFriend() {}

        public UserFriend(User friend, bool allowCommunication) {
            Friend = friend;
            AllowCommunication = allowCommunication;
        }
    }
}
