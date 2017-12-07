using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Satochat.Server.Models
{
    public class UserCredential {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; }
        public string PasswordDigest { get; set; }

        public virtual User User { get; set; }

        public UserCredential() { }

        public UserCredential(string username, string passwordDigest) {
            Username = username;
            PasswordDigest = passwordDigest;
        }

        public UserCredential(User user,  string username, string passwordDigest) {
            User = user;
            Username = username;
            PasswordDigest = passwordDigest;
        }
    }
}
