using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Satochat.Server.Models {
    public class ConversationMessage {
        public int Id { get; set; }
        public int ConversationId { get; set; }
        public int MessageId { get; set; }

        public virtual Conversation Conversation { get; set; }
        public virtual Message Message { get; set; }
    }
}
