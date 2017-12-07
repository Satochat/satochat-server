using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Satochat.Server.Models {
    public class ConversationUser {
        public int Id { get; set; }
        public int ConversationId { get; set; }
        public int UserId { get; set; }
        public DateTimeOffset LastTimeTyping { get; set; }

        //public virtual Conversation Conversation { get; set; }
        public virtual User User { get; set; }

        public ConversationUser() {}

        public ConversationUser(User user) {
            User = user;
        }
    }
}
