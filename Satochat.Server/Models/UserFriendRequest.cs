using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Satochat.Server.Models {
    public class UserFriendRequest {
        public int Id { get; set; }
        public int InitiatorId { get; set; }
        public int FriendId { get; set; }
        //public bool? FriendAnswer { get; set; }
        public DateTimeOffset Expiry { get; set; }

        public virtual User Initiator { get; set; }
        public virtual User Friend { get; set; }
    }
}
