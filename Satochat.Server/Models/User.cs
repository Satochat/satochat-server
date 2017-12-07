using System;
using System.Collections.Generic;

namespace Satochat.Server.Models {
    public class User {
        public int Id { get; set; }
        public string Uuid { get; set; } = Guid.NewGuid().ToString();

        public virtual ICollection<AccessToken> AccessTokens { get; set; } = new List<AccessToken>();
        public virtual ICollection<UserCredential> Credentials { get; set; } = new List<UserCredential>();
        public virtual ICollection<UserFriend> Friends { get; set; } = new List<UserFriend>();
        public virtual ICollection<UserPublicKey> PublicKeys { get; set; } = new List<UserPublicKey>();
        public virtual ICollection<UserEvent> Events { get; set; } = new List<UserEvent>();
    }
}
