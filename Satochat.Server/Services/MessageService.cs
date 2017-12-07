using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Satochat.Server.Models;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Satochat.Shared.Event;
using Satochat.Shared.Util;

namespace Satochat.Server.Services
{
    public class MessageService : IMessageService {

        private readonly ILogger<MessageService> _logger;

        private readonly SatochatContext _dbContext;
        private readonly IEventService _eventService;

        public MessageService(ILogger<MessageService> logger, SatochatContext dbContext, IEventService eventService) {
            _logger = logger;
            _dbContext = dbContext;
            _eventService = eventService;
        }

        public async Task<Conversation> CreateConversationAsync(User[] participants) {
            var participantsUuids = participants.Select(e => e.Uuid);
            var conversationUuid = ConversationUtil.GenerateUuid(participantsUuids);
            
            var conversation = new Conversation();
            conversation.Uuid = conversationUuid;

            foreach (var user in participants) {
                conversation.Users.Add(new ConversationUser(user));
            }

            _dbContext.Conversations.Add(conversation);
            await _dbContext.SaveChangesAsync();
            return conversation;
        }

        public async Task<Conversation> FindConversationByIdAsync(int id) {
            return await _dbContext.Conversations.Include(e => e.Users).SingleOrDefaultAsync(e => e.Id == id);
        }

        public async Task<Conversation> FindConversationByUuidAsync(string uuid) {
            return await _dbContext.Conversations.Include(e => e.Users).SingleOrDefaultAsync(e => e.Uuid == uuid);
        }

        public async Task<Conversation> FindConversationByUsersAsync(User[] users) {
            if (users.Length == 0) {
                // TODO: throw
                return null;
            }

            var participantsUuids = users.Select(e => e.Uuid);
            var conversationUuid = ConversationUtil.GenerateUuid(participantsUuids);

            var conversation = await _dbContext.Conversations.Include(e => e.Users).SingleOrDefaultAsync(e => e.Uuid == conversationUuid);
            return conversation;

            /*string userIdList = String.Join(", ", users.Select(e => e.Id));
            string sql = "SELECT * FROM ConversationUsers GROUP BY ConversationId HAVING SUM(UserId IN ({0})) = COUNT(*) AND COUNT(*) = {1}";
            sql = String.Format(sql, userIdList, users.Length);
            var temp = _dbContext.ConversationUsers.FromSql(sql).SingleOrDefault();
            if (temp == null) {
                // No conversation exists
                return null;
            }

            var conversation = _dbContext.Conversations.Include(e => e.Users).Single(e => e.Id == temp.ConversationId);
            return conversation;*/
        }

        public async Task<Conversation> FindConversationForUserAsync(User user, string uuid) {
            var conversation = await _dbContext.Conversations.Include(e => e.Users).SingleOrDefaultAsync(e => e.Uuid == uuid);
            if (conversation == null) {
                return null;
            }

            var participant = conversation.Users.SingleOrDefault(e => e.UserId == user.Id);
            if (participant == null) {
                return null;
            }

            return conversation;
        }

        public async Task<EncodedMessage> FindMessageForUserAsync(User user, string uuid) {
            var message = await _dbContext.Messages.SingleOrDefaultAsync(e => e.Uuid == uuid);
            if (message == null) {
                return null;
            }

            var conversation = await _dbContext.Conversations.Include(e => e.Users).SingleOrDefaultAsync(e => e.Id == message.ConversationId);
            if (conversation == null) {
                return null;
            }

            var recipient = conversation.Users.SingleOrDefault(e => e.Id == message.RecipientId);
            if (recipient == null) {
                return null;
            }

            if (user.Id != recipient.UserId) {
                return null;
            }

            return message;
        }

        public void AddMessages(IEnumerable<EncodedMessage> messages, Conversation conversation) {
            foreach (var message in messages) {
                conversation.Messages.Add(message);
            }

            _dbContext.SaveChanges();
        }

        public IEnumerable<EncodedMessage> GetMessages(Conversation conversation, ConversationUser recipient) {
            return _dbContext.Messages.Where(e => e.ConversationId == conversation.Id && e.RecipientId == recipient.Id);
        }

        public bool ConversationHasParticipant(Conversation conversation, User user) {
            return conversation.Users.SingleOrDefault(e => e.UserId == user.Id) != null;
        }

        /*public EncodedMessage FindMessageByUuid(string uuid) {
            throw new NotImplementedException();
        }*/

        public async Task MessageReceivedAsync(string messageUuid, string userUuid) {
            var message = await _dbContext.Messages.Include(e => e.Recipient).Include(e => e.Author).SingleOrDefaultAsync(e => e.Uuid == messageUuid);
            if (message == null) {
                throw new ServiceException(ServiceErrorCode.NotFound);
            }

            var user = await _dbContext.Users.SingleOrDefaultAsync(e => e.Uuid == userUuid);
            if (user == null) {
                throw new ServiceException(ServiceErrorCode.NotFound);
            }

            if (message.Recipient.UserId != user.Id) {
                // Not this user's message
                throw new ServiceException(ServiceErrorCode.Unauthorized);
            }

            _dbContext.Messages.Remove(message);
            await _dbContext.SaveChangesAsync();

            var conversation = await _dbContext.Conversations.SingleOrDefaultAsync(e => e.Id == message.ConversationId);
            if (conversation == null) {
                // Return success because the recipient does not need to care about the rest
                throw new ServiceException(ServiceErrorCode.Success);
            }

            // Notify the author user that the message was received by the recipient user
            var author = await _dbContext.Users.SingleOrDefaultAsync(e => e.Id == message.Author.UserId);
            if (author == null) {
                // Return success because the recipient does not need to care about the rest
                throw new ServiceException(ServiceErrorCode.Success);
            }

            await _eventService.AddAsync(author, new MessageEvent.UserReceivedMessage(userUuid, conversation.Uuid, messageUuid));
        }
    }
}
