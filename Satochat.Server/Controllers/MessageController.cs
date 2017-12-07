using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Satochat.Server.Services;
using Microsoft.AspNetCore.Mvc;
using Satochat.Server.Models;
using Satochat.Server.ViewModels;
using Satochat.Shared.Event;
using Satochat.Shared.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Satochat.Server.Controllers {
    [Route("message")]
    public class MessageController : BaseController {
        private readonly IUserService _userService;
        private readonly IMessageService _messageService;
        private readonly IEventService _eventService;
        private readonly SatochatContext _dbContext;

        public MessageController(IUserService userService, IMessageService messageService, IEventService eventService, SatochatContext dbContext) {
            _userService = userService;
            _messageService = messageService;
            _eventService = eventService;
            _dbContext = dbContext;
        }

        [HttpGet]
        [Route("{uuid}")]
        public async Task<IActionResult> Get(string uuid) {
            var user = await _userService.FindUserByUuidAsync(getUserUuid());
            var msg = await _messageService.FindMessageForUserAsync(user, uuid);
            if (msg == null) {
                return NotFound();
            }

            var conversation = await _messageService.FindConversationByIdAsync(msg.ConversationId);
            if (conversation == null) {
                return NotFound();
            }

            var author = await _dbContext.Users.SingleOrDefaultAsync(e => e.Id == msg.Author.UserId);
            if (author == null) {
                return NotFound();
            }

            var message = new MessageViewModelAspnet.IncomingEncodedMessage(msg.Uuid, author.Uuid, msg.Digest, msg.Payload, msg.Iv, msg.Key, msg.Timestamp);;
            var result = new MessageViewModelAspnet.GetMessageResult(conversation.Uuid, message);
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> Get(MessageViewModelAspnet.GetMessages model) {
            var user = await _userService.FindUserByUuidAsync(getUserUuid());
            var conversation = await _messageService.FindConversationByUuidAsync(model.Conversation);
            if (conversation == null) {
                return NotFound();
            }

            var conversationUser = conversation.Users.SingleOrDefault(e => e.UserId == user.Id);
            if (conversationUser == null) {
                return StatusCode(403);
            }

            var encodedMessages = _messageService.GetMessages(conversation, conversationUser);

            var response = new MessageViewModelAspnet.GetMessagesResult();
            foreach (var msg in encodedMessages) {
                var message = new MessageViewModelAspnet.IncomingEncodedMessage(msg.Uuid, user.Uuid, msg.Digest, msg.Payload,
                    msg.Iv, msg.Key, msg.Timestamp);
                response.Messages.Add(message);
            }

            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody]MessageViewModelAspnet.SendMessage model) {
            var author = await _userService.FindUserByUuidAsync(getUserUuid());
            var recipients = model.Variants.Select(variant => variant.Recipient).Select(uuid => _userService.FindUserByUuidAsync(uuid)).Select(t => t.Result);
            bool allRecipientsFound = recipients.All(e => e != null);

            if (!allRecipientsFound) {
                // One or more recipients are unknown
                return NotFound();
            }

            var participants = new List<User>();
            participants.Add(author);
            participants.AddRange(recipients);

            bool friends = await _userService.UsersAreFriendsAsync(participants.ToArray());
            if (!friends) {
                // All participants must be friends
                return StatusCode(403);
            }

            Conversation conversation;
            using (var transaction = await _dbContext.Database.BeginTransactionAsync()) {
                conversation = await _messageService.FindConversationByUsersAsync(participants.ToArray()) ?? await _messageService.CreateConversationAsync(participants.ToArray());
                transaction.Commit();
            }

            var items = new List<MessageViewModel.SendMessageResultItem>();

            List<EncodedMessage> messages = new List<EncodedMessage>();
            long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var messageAuthor = conversation.Users.Single(e => e.UserId == author.Id);
            foreach (var variant in model.Variants) {
                var messageRecipient = conversation.Users.Single(e => e.User.Uuid == variant.Recipient);
                var message = new EncodedMessage(messageAuthor, messageRecipient, variant.Digest, variant.Payload, variant.Iv, variant.Key) {
                    Timestamp = timestamp
                };
                items.Add(new MessageViewModel.SendMessageResultItem(messageRecipient.User.Uuid, message.Uuid));
                messages.Add(message);
            }

            _messageService.AddMessages(messages, conversation);

            foreach (var message in messages) {
                await _eventService.AddAsync(message.Recipient.User, new MessageEvent.NewMessageAvailable(message.Uuid));
            }

            return Ok(new MessageViewModel.SendMessageResult(conversation.Uuid, items));
        }

        [HttpPost]
        [Route("received/{uuid}")]
        public async Task<IActionResult> PostMessageReceived(string uuid) {
            await _messageService.MessageReceivedAsync(uuid, getUserUuid());
            //var user = _userService.FindUserByUuid(getUserUuid());
            //_eventService.AddAsync(user, );
            return Ok();
        }
    }
}
