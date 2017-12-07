using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Satochat.Server.Services;
using Satochat.Server.Models;
using Satochat.Server.ViewModels;
using System;
using Satochat.Shared.Event;
using Microsoft.EntityFrameworkCore;

namespace Satochat.Server.Controllers {
    [Route("conversation")]
    public class ConversationController : BaseController {
        private readonly IUserService _userService;
        private readonly IMessageService _messageService;
        private readonly IEventService _eventService;
        private readonly SatochatContext _dbContext;

        public ConversationController(IUserService userService, IMessageService messageService, IEventService eventService, SatochatContext dbContext) {
            _userService = userService;
            _messageService = messageService;
            _eventService = eventService;
            _dbContext = dbContext;
        }

        [HttpGet]
        [Route("{uuid}")]
        public async Task<IActionResult> Get(string uuid) {
            var user = await _userService.FindUserByUuidAsync(getUserUuid());
            var conversation = await _messageService.FindConversationForUserAsync(user, uuid);
            if (conversation == null) {
                return NotFound();
            }

            if (!conversation.Users.Any()) {
                return NotFound();
            }
            
            List<User> participants = new List<User>();
            foreach (var conversationUser in conversation.Users) {
                var participant = await _dbContext.Users.SingleOrDefaultAsync(e => e.Id == conversationUser.UserId);
                if (participant == null) {
                    return NotFound();
                }

                participants.Add(participant);
            }

            var participantUuids = participants.Select(e => e.Uuid).ToArray();
            if (!participantUuids.Any()) {
                return NotFound();
            }

            if (!participantUuids.Contains(user.Uuid)) {
                return StatusCode(403);
            }

            return Ok(new ConversationViewModelAspnet.GetConversationResult(participantUuids));
        }

        [HttpPost]
        [Route("typing/{uuid}")]
        public async Task<IActionResult> PostTyping(string uuid) {
            using (var transaction = await _dbContext.Database.BeginTransactionAsync()) {
                var user = await _userService.FindUserByUuidAsync(getUserUuid());
                var conversation = await _messageService.FindConversationForUserAsync(user, uuid);
                if (conversation == null) {
                    return NotFound();
                }

                var users = new List<User>();
                foreach (var participant in conversation.Users) {
                    users.Add(_dbContext.Users.Single(e => e.Id == participant.UserId));
                }

                var author = conversation.Users.Single(e => e.UserId == user.Id);
                if (author == null) {
                    return Ok();
                }

                var otherUsers = users.Where(e => e.Id != author.UserId).ToArray();

                var lastTimeTyping = author.LastTimeTyping;
                var currentTime = DateTimeOffset.UtcNow;

                if (currentTime - lastTimeTyping < TimeSpan.FromSeconds(1)) {
                    return Ok();
                }

                author.LastTimeTyping = currentTime;
                await _dbContext.SaveChangesAsync();

                var userEvent = new MessageEvent.UserTypingInConversation(user.Uuid, conversation.Uuid);
                foreach (var otherUser in otherUsers) {
                    await _eventService.AddAsync(otherUser, userEvent);
                }

                transaction.Commit();
            }

            return Ok();
        }

        /*[HttpPost]
        public async Task<IActionResult> Post([FromBody]ConversationViewModel.Create model) {
            var author = _userService.FindUserByUuid(getUserUuid());
            if (model.Recipients.Any(uuid => uuid == author.Uuid)) {
                ModelState.AddModelError(nameof(model.Recipients), "Author cannot be a recipient");
                return BadRequest(ModelState);
            }

            var recipients = model.Recipients.Select(uuid => _userService.FindUserByUuid(uuid));
            bool allRecipientsFound = recipients.All(e => e != null);
            if (!allRecipientsFound) {
                // One or more recipients are unknown
                return NotFound();
            }

            var participants = new List<User>();
            participants.Add(author);
            participants.AddRange(recipients);

            bool friends = _userService.UsersAreFriends(participants.ToArray());
            if (!friends) {
                // All participants must be friends
                return StatusCode(403);
            }

            var conversation = _messageService.FindConversationByUsers(participants.ToArray());
            if (conversation == null) {
                conversation = _messageService.CreateConversation(participants.ToArray());
            }

            //_notificationService.Send(new MessageNotification.ConversationCreated(conversation.Uuid));

            return Ok(new ConversationViewModel.CreateResult(conversation.Uuid));
        }*/
    }
}