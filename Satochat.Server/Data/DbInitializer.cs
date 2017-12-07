using Satochat.Server.Helper;
using Satochat.Server.Models;

namespace Satochat.Server.Data {
    public static class DbInitializer {
        public static void Initialize(SatochatContext context) {
            //context.Database.EnsureDeleted();
            //context.Database.EnsureCreated();
            //seed(context);
        }

        private static void seed(SatochatContext context) {
            var satoshi = new User();
            satoshi.Credentials.Add(new UserCredential("satoshi", UserCredentialHelper.HashPassword("12345678")));

            var sachiko = new User();
            sachiko.Credentials.Add(new UserCredential("sachiko", UserCredentialHelper.HashPassword("12345678")));

            //var dummy = new User();
            //dummy.Credentials.Add(new UserCredential("dummy", UserCredentialHelper.HashPassword("12345678")));

            satoshi.Friends.Add(new UserFriend(sachiko, false));
            //satoshi.Friends.Add(new UserFriend(dummy, false));

            sachiko.Friends.Add(new UserFriend(satoshi, false));
            //sachiko.Friends.Add(new UserFriend(dummy, false));

            //dummy.Friends.Add(new UserFriend(satoshi, false));
            //dummy.Friends.Add(new UserFriend(sachiko, false));

            context.Users.Add(satoshi);
            context.Users.Add(sachiko);
            //context.Users.Add(dummy);

            /*var directConversation = new Conversation();
            directConversation.Users.Add(new ConversationUser(satoshi));
            directConversation.Users.Add(new ConversationUser(sachiko));
            context.Conversations.Add(directConversation);

            var manyConversation = new Conversation();
            manyConversation.Users.Add(new ConversationUser(satoshi));
            manyConversation.Users.Add(new ConversationUser(sachiko));
            manyConversation.Users.Add(new ConversationUser(dummy));
            context.Conversations.Add(manyConversation);*/

            context.SaveChanges();
        }
    }
}
