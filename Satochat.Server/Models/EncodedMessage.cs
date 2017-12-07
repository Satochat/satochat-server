using System;

namespace Satochat.Server.Models {
    public class EncodedMessage {
        public int Id { get; set; }
        public int ConversationId { get; set; }
        public int AuthorId { get; set; }
        public int RecipientId { get; set; }
        public string Uuid { get; set; } = Guid.NewGuid().ToString();
        public string Digest { get; set; }
        public string Payload { get; set; }
        public string Iv { get; set; }
        public string Key { get; set; }
        public long Timestamp { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        //public virtual Conversation Conversation { get; set; }
        public virtual ConversationUser Author { get; set; }
        public virtual ConversationUser Recipient { get; set; }

        public EncodedMessage() {}

        public EncodedMessage(ConversationUser author, ConversationUser recipient, string digest, string payload, string iv, string key) {
            Author = author;
            Recipient = recipient;
            Digest = digest;
            Payload = payload;
            Iv = iv;
            Key = key;
        }
    }
}
