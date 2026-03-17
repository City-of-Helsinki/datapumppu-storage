namespace Storage.Actions
{
    /// <summary>
    /// Defines the contract for the event action dispatcher that routes events to their handlers.
    /// </summary>
    public interface IEventActions
    {
        /// <summary>
        /// Retrieves all action handlers registered for a specific event type.
        /// </summary>
        /// <param name="eventType">The event type to find handlers for.</param>
        /// <returns>A list of action handlers that can process the specified event type.</returns>
        List<IEventAction> GetActionsForEvent(EventType eventType);
    }

    /// <summary>
    /// Dispatches incoming events to appropriate action handlers based on event type.
    /// This class acts as a router that maps EventType values to their registered IEventAction implementations.
    /// </summary>
    public class EventActions : IEventActions
    {
        private readonly IEnumerable<IEventAction> _eventActions;

        /// <summary>
        /// Initializes a new instance of the EventActions dispatcher with the collection of available action handlers.
        /// </summary>
        /// <param name="eventActions">The collection of all registered event action handlers, typically injected via dependency injection.</param>
        public EventActions(IEnumerable<IEventAction> eventActions)
        {
            _eventActions = eventActions;
        }

        /// <summary>
        /// Retrieves all action handlers registered for a specific event type.
        /// Multiple handlers can be registered for the same event type to perform different operations.
        /// </summary>
        /// <param name="eventType">The event type to find handlers for.</param>
        /// <returns>A list of action handlers that have the specified event type in their EventTypes property.</returns>
        public List<IEventAction> GetActionsForEvent(EventType eventType)
        {
            return _eventActions.Where(eventAction => eventAction.EventTypes.Contains(eventType)).ToList();
        }
    }
}
