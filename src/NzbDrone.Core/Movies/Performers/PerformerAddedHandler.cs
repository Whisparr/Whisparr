using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Movies.Performers.Commands;
using NzbDrone.Core.Movies.Performers.Events;

namespace NzbDrone.Core.Movies.Performers
{
    public class PerformerAddedHandler : IHandle<PerformerAddedEvent>, IHandle<PerformersAddedEvent>
    {
        private readonly IManageCommandQueue _commandQueueManager;

        public PerformerAddedHandler(IManageCommandQueue commandQueueManager)
        {
            _commandQueueManager = commandQueueManager;
        }

        public void Handle(PerformerAddedEvent message)
        {
            _commandQueueManager.Push(new RefreshPerformersCommand(new List<int> { message.Performer.Id }));
        }

        public void Handle(PerformersAddedEvent message)
        {
            _commandQueueManager.PushMany(message.Performers.Select(s => new RefreshPerformersCommand(new List<int> { s.Id })).ToList());
        }
    }
}
