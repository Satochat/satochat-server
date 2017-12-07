namespace Satochat.Server.Models {
    public class UserPublicKey {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Key { get; set; }

        public virtual User User { get; set; }

        public UserPublicKey() { }

        public UserPublicKey(string key) {
            Key = key;
        }
    }
}
