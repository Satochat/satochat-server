using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Satochat.Server.Models;
using Microsoft.EntityFrameworkCore;
using Satochat.Server.Services;
using Satochat.Shared.Event;
using System.Linq;
using Satochat.Server.ViewModels;

namespace Satochat.Server.Controllers {
    [Route("event")]
    public class EventController : BaseController {
        private readonly SatochatContext _dbContext;
        private readonly IEventService _eventService;

        public EventController(SatochatContext dbContext, IEventService eventService) {
            _dbContext = dbContext;
            _eventService = eventService;
        }

        [HttpGet]
        public async Task<IActionResult> Get() {
            Response.ContentType = "text/event-stream";
            bool hasGreeted = false;
            DateTimeOffset? lastPingTime = null;
            var sentToClient = new SortedSet<string>();
            
            var user = await _dbContext.Users.SingleOrDefaultAsync(e => e.Uuid == getUserUuid());
            if (user == null) {
                throw new ServiceException(ServiceErrorCode.NotFound);
            }

            do {
                // Send initial ping and then regularly
                if (!lastPingTime.HasValue || DateTimeOffset.Now - lastPingTime.Value >= TimeSpan.FromSeconds(10)) {
                    await sendEvent(null, "ping", null);
                    lastPingTime = DateTimeOffset.Now;
                }

                if (HttpContext.RequestAborted.IsCancellationRequested) {
                    break;
                }

                if (!hasGreeted) {
                    await sendInitialEvents(user);
                    hasGreeted = true;
                }

                var userEvents = (await _eventService.PeekAllAsync(user)).ToArray();
                if (!userEvents.Any()) {
                    _dbContext.Database.CloseConnection();
                    // This might get pretty heavy with  many connections
                    // We just do not want to hold the DB connection open for long.
                    // FIXME: Find some way to improve performance without delaying?
                    await Task.Delay(TimeSpan.FromMilliseconds(100));
                    continue;
                }

                sentToClient.RemoveWhere(uuid => userEvents.All(e => e.GetUuid() != uuid));

                foreach (var userEvent in userEvents) {
                    if (sentToClient.Contains(userEvent.GetUuid())) {
                        continue;
                    }
                
                    await sendEvent(userEvent.GetUuid(), userEvent.GetName(), userEvent.GetData());
                    sentToClient.Add(userEvent.GetUuid());
                }
                
                _dbContext.Database.CloseConnection();
                //await Task.Delay(TimeSpan.FromSeconds(1));
            } while (!HttpContext.RequestAborted.IsCancellationRequested);
            
            _dbContext.Database.CloseConnection();
            return new EventActionResult();
        }

        [HttpPost]
        [Route("received/{uuid}")]
        public async Task<IActionResult> PostReceived(string uuid) {
            await _eventService.EventReceivedAsync(uuid, getUserUuid());
            return Ok();
        }

        private async Task sendInitialEvents(User user) {
            var friends = await _dbContext.UserFriends.Where(e => e.UserId == user.Id).Include(e => e.Friend).ToArrayAsync();
            var friendUuids = friends.Select(e => e.Friend.Uuid).ToArray();
            var userEvent = new FriendEvent.FriendList(friendUuids);
            await sendEvent(user.Uuid, userEvent.GetName(), userEvent.GetData());
        }

        /*
        private async Task sendEvent(string name, object data) {
            var dataJson = JsonConvert.SerializeObject(data);
            var model = new EventViewModelAspnet.PushEvent(name, dataJson);
            var json = JsonConvert.SerializeObject(model);
            await Response.WriteAsync(json + "\n\n");
            await Response.Body.FlushAsync();
        }
        */

        private async Task sendEvent(string uuid, string name, string data) {
            var model = new EventViewModelAspnet.PushEvent(uuid, name, data);
            var json = JsonConvert.SerializeObject(model);
            await Response.WriteAsync(json + "\n\n");
            await Response.Body.FlushAsync();
        }

        private class EventActionResult : ActionResult {
            
        }
    }
}