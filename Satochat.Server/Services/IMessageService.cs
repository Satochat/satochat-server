using Satochat.Server.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Satochat.Server.Services {
    public interface IMessageService {
        Task<Conversation> FindConversationByIdAsync(int id);
        Task<Conversation> FindConversationByUuidAsync(string uuid);
        Task<Conversation> FindConversationByUsersAsync(User[] users);
        Task<Conversation> FindConversationForUserAsync(User user, string uuid);
        Task<Conversation> CreateConversationAsync(User[] participants);
        void AddMessages(IEnumerable<EncodedMessage> messages, Conversation conversation);
        IEnumerable<EncodedMessage> GetMessages(Conversation conversation, ConversationUser recipient);
        bool ConversationHasParticipant(Conversation conversation, User user);
        //EncodedMessage FindMessageByUuid(string uuid);
        Task MessageReceivedAsync(string messageUuid, string userUuid);
        Task<EncodedMessage> FindMessageForUserAsync(User user, string uuid);
    }
}
