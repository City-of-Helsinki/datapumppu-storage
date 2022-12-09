namespace Storage.Actions
{
    public interface IEventActions
    {
        List<IEventAction> GetActionsForEvent(EventType eventType);
    }

    public class EventActions : IEventActions
    {
        private readonly IEnumerable<IEventAction> _eventActions;

        public EventActions(IEnumerable<IEventAction> eventActions)
        {
            _eventActions = eventActions;
        }

        public List<IEventAction> GetActionsForEvent(EventType eventType)
        {
            return _eventActions.Where(eventAction => eventAction.EventTypes.Contains(eventType)).ToList();
        }
    }
}
