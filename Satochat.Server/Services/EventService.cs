using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Satochat.Shared.Event;
using Satochat.Server.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Satochat.Server.Services {
    public class EventService : IEventService {
        //private readonly ConcurrentDictionary<string, BlockingCollection<IEvent>> _userQueues = new ConcurrentDictionary<string, BlockingCollection<IEvent>>();

        private readonly SatochatContext _dbContext;

        public EventService(SatochatContext dbContext) {
            _dbContext = dbContext;
        }

        public async Task AddAsync(User user, IEvent e) {
            /*BlockingCollection<IEvent> queue = null;
            lock (_userQueues) {
                if (!_userQueues.TryGetValue(userUuid, out queue)) {
                    queue = new BlockingCollection<IEvent>();
                    _userQueues.TryAdd(userUuid, queue);
                }
            }
            if (queue != null) {
                queue.Add(e);
            }*/

            _dbContext.UserEvents.Add(new UserEvent {
                Name = e.GetName(),
                Data = e.GetData(),
                User = user,
                Priority = 1
            });
            await _dbContext.SaveChangesAsync();
        }

        public async Task AddFirstAsync(User user, IEvent e) {
            _dbContext.UserEvents.Add(new UserEvent {
                Name = e.GetName(),
                Data = e.GetData(),
                User = user,
                Priority = 0
            });
            await _dbContext.SaveChangesAsync();
        }

        public async Task<IEvent> PeekAsync(User user) {
            var userEvent = await _dbContext.UserEvents.OrderBy(e => e.Priority).ThenBy(e => e.Timestamp).FirstOrDefaultAsync(e => e.UserId == user.Id);
            return userEvent;
            /*BlockingCollection<IEvent> queue = null;
            lock (_userQueues) {
                if (!_userQueues.TryGetValue(userUuid, out queue)) {
                    return null;
                }
            }
            return await Task.Run(() => {
                return queue.Take();
            });*/
        }

        public async Task<IEnumerable<IEvent>> PeekAllAsync(User user) {
            var userEvents = await _dbContext.UserEvents.Where(e => e.UserId == user.Id).OrderBy(e => e.Priority).ThenBy(e => e.Timestamp).ToArrayAsync();
            return userEvents;
        }

        public async Task RemoveAsync(IEvent e) {
            var userEvent = await _dbContext.UserEvents.SingleOrDefaultAsync(entity => entity.Uuid == e.GetUuid());
            if (userEvent == null) {
                return;
            }

            _dbContext.UserEvents.Remove(userEvent);
            await _dbContext.SaveChangesAsync();
        }

        public async Task EventReceivedAsync(string eventUuid, string userUuid) {
            var userEvent = await _dbContext.UserEvents.SingleOrDefaultAsync(e => e.Uuid == eventUuid);
            if (userEvent == null) {
                throw new ServiceException(ServiceErrorCode.NotFound);
            }

            var user = await _dbContext.Users.SingleOrDefaultAsync(e => e.Uuid == userUuid);
            if (user == null) {
                throw new ServiceException(ServiceErrorCode.NotFound);
            }

            if (userEvent.UserId != user.Id) {
                // Not this user's event
                throw new ServiceException(ServiceErrorCode.Unauthorized);
            }

            _dbContext.UserEvents.Remove(userEvent);
            await _dbContext.SaveChangesAsync();
        }
    }
}
