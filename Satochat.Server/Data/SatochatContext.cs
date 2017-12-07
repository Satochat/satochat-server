using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Satochat.Server.Models {
    public class SatochatContext : DbContext {
        public DbSet<AccessToken> AccessTokens { get; set; }
        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<ConversationUser> ConversationUsers { get; set; }
        public DbSet<EncodedMessage> Messages { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserCredential> UserCredentials { get; set; }
        public DbSet<UserFriend> UserFriends { get; set; }
        public DbSet<UserFriendRequest> UserFriendRequest { get; set; }
        public DbSet<UserEvent> UserEvents { get; set; }
        public DbSet<UserPublicKey> UserPublicKeys { get; set; }

        public SatochatContext(DbContextOptions<SatochatContext> options)
            : base(options) {
        }

        protected override void OnModelCreating(ModelBuilder builder) {
            // AccessToken
            builder.Entity<AccessToken>().HasKey(e => e.Id);
            builder.Entity<AccessToken>().HasOne(e => e.User).WithMany(e => e.AccessTokens).HasForeignKey(e => e.UserId).IsRequired().OnDelete(DeleteBehavior.Cascade);
            builder.Entity<AccessToken>().Property(e => e.Expiry).IsRequired();
            builder.Entity<AccessToken>().Property(e => e.Token).IsRequired();
            builder.Entity<AccessToken>().Property(e => e.IpAddress).IsRequired();
            builder.Entity<AccessToken>().HasIndex(e => e.Token).IsUnique();
            builder.Entity<AccessToken>().HasIndex(e => new { e.UserId, e.IpAddress });

            // Conversation
            builder.Entity<Conversation>().HasKey(e => e.Id);
            builder.Entity<Conversation>().Property(e => e.Uuid).IsRequired();
            builder.Entity<Conversation>().HasIndex(e => e.Uuid).IsUnique();
            //builder.Entity<Conversation>().HasMany(e => e.Messages).WithOne().HasForeignKey(e => e.ConversationId).IsRequired().OnDelete(DeleteBehavior.Cascade);
            builder.Entity<Conversation>().HasMany(e => e.Users).WithOne().HasForeignKey(e => e.ConversationId).IsRequired().OnDelete(DeleteBehavior.Cascade);

            // ConversationUser
            builder.Entity<ConversationUser>().HasKey(e => e.Id);
            //builder.Entity<ConversationUser>().HasKey(e => new { e.ConversationId, e.UserId });
            builder.Entity<ConversationUser>().HasIndex(e => new { e.ConversationId, e.UserId }).IsUnique();
            //builder.Entity<ConversationUser>().HasOne(e => e.Conversation).WithMany().HasForeignKey(e => e.ConversationId).IsRequired().OnDelete(DeleteBehavior.Cascade);
            //builder.Entity<ConversationUser>().HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId).IsRequired().OnDelete(DeleteBehavior.Cascade);

            // EncodedMessage
            builder.Entity<EncodedMessage>().HasKey(e => e.Id);
            //builder.Entity<EncodedMessage>().HasOne(e => e.Conversation).WithMany(e => e.Messages).HasForeignKey(e => e.ConversationId).IsRequired().OnDelete(DeleteBehavior.Cascade);
            builder.Entity<EncodedMessage>().HasOne(e => e.Author).WithMany().HasForeignKey(e => e.AuthorId).IsRequired().OnDelete(DeleteBehavior.Cascade);
            builder.Entity<EncodedMessage>().HasOne(e => e.Recipient).WithMany().HasForeignKey(e => e.RecipientId).IsRequired().OnDelete(DeleteBehavior.Cascade);
            builder.Entity<EncodedMessage>().Property(e => e.Uuid).IsRequired();
            builder.Entity<EncodedMessage>().Property(e => e.Digest).IsRequired();
            builder.Entity<EncodedMessage>().Property(e => e.Payload).IsRequired();

            // User
            builder.Entity<User>().HasKey(e => e.Id);
            builder.Entity<User>().HasIndex(e => e.Uuid).IsUnique();

            // UserCredential
            builder.Entity<UserCredential>().HasKey(e => e.Id);
            builder.Entity<UserCredential>().Property(e => e.Username).IsRequired();
            builder.Entity<UserCredential>().Property(e => e.PasswordDigest).IsRequired();
            builder.Entity<UserCredential>().HasIndex(e => e.Username).IsUnique();
            builder.Entity<UserCredential>().HasOne(e => e.User).WithMany(e => e.Credentials).HasForeignKey(e => e.UserId).IsRequired().OnDelete(DeleteBehavior.Cascade);

            // UserFriend
            builder.Entity<UserFriend>().HasKey(e => e.Id);
            builder.Entity<UserFriend>().Property(e => e.AllowCommunication).IsRequired();
            builder.Entity<UserFriend>().HasOne(e => e.User).WithMany(e => e.Friends).HasForeignKey(e => e.UserId).IsRequired().OnDelete(DeleteBehavior.Cascade);
            builder.Entity<UserFriend>().HasOne(e => e.Friend).WithMany().HasForeignKey(e => e.FriendId).IsRequired().OnDelete(DeleteBehavior.Cascade);
            builder.Entity<UserFriend>().HasIndex(e => new { e.UserId, e.FriendId });

            // Event
            builder.Entity<UserEvent>().HasKey(e => e.Id);
            builder.Entity<UserEvent>().HasOne(e => e.User).WithMany(e => e.Events).HasForeignKey(e => e.UserId).IsRequired().OnDelete(DeleteBehavior.Cascade);

            // UserPublicKey
            builder.Entity<UserPublicKey>().HasKey(e => e.Id);
            builder.Entity<UserPublicKey>().HasOne(e => e.User).WithMany(e => e.PublicKeys).HasForeignKey(e => e.UserId).IsRequired().OnDelete(DeleteBehavior.Cascade);
        }
    }
}
