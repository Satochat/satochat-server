using System.Collections;
using System.Collections.Generic;
using Satochat.Server.Models;
using System.Threading.Tasks;

namespace Satochat.Shared.Event {
    public interface IEventService {
        Task AddAsync(User user, IEvent e);
        Task AddFirstAsync(User user, IEvent e);
        Task<IEvent> PeekAsync(User user);
        Task<IEnumerable<IEvent>> PeekAllAsync(User user);
        Task RemoveAsync(IEvent e);
        Task EventReceivedAsync(string eventUuid, string userUuid);
    }
}
