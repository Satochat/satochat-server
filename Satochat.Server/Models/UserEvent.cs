using Satochat.Shared.Event;
using System;

namespace Satochat.Server.Models {
    public class UserEvent : IEvent {
        public int Id { get; set; }
        public string Uuid { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; }
        public string Data { get; set; }
        public long Timestamp { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        public int UserId { get; set; }
        public int Priority { get; set; }
        
        public virtual User User { get; set; }

        public string GetUuid() {
            return Uuid;
        }

        public string GetData() {
            return Data;
        }

        public string GetName() {
            return Name;
        }
    }
}
