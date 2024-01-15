using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Movies.Studios.Commands;
using NzbDrone.Core.Movies.Studios.Events;

namespace NzbDrone.Core.Movies.Studios
{
    public class StudioAddedHandler : IHandle<StudioAddedEvent>, IHandle<StudiosAddedEvent>
    {
        private readonly IManageCommandQueue _commandQueueManager;

        public StudioAddedHandler(IManageCommandQueue commandQueueManager)
        {
            _commandQueueManager = commandQueueManager;
        }

        public void Handle(StudioAddedEvent message)
        {
            _commandQueueManager.Push(new RefreshStudiosCommand(new List<int> { message.Studio.Id }));
        }

        public void Handle(StudiosAddedEvent message)
        {
            _commandQueueManager.PushMany(message.Studios.Select(s => new RefreshStudiosCommand(new List<int> { s.Id })).ToList());
        }
    }
}
