using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Satochat.Server.Models {
    public class Conversation {
        public int Id { get; set; }
        public string Uuid { get; set; } = Guid.NewGuid().ToString();

        public virtual ICollection<EncodedMessage> Messages { get; set; } = new List<EncodedMessage>();
        public virtual ICollection<ConversationUser> Users { get; set; } = new List<ConversationUser>();
    }
}
