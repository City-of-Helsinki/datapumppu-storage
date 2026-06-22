using System.Data;

namespace Storage.Actions
{
    /// <summary>
    /// Defines the contract for event action handlers that process incoming events from Kafka.
    /// Each implementation handles specific event types and executes database operations within a transaction.
    /// </summary>
    public interface IEventAction
    {
        /// <summary>
        /// Gets the list of event types that this action can handle.
        /// Used by the event dispatcher to route events to appropriate handlers.
        /// </summary>
        public List<EventType>? EventTypes { get; }

        /// <summary>
        /// Executes the action logic for a received event within a database transaction.
        /// Implementations should deserialize the event body, validate data, and perform database operations.
        /// </summary>
        /// <param name="eventBody">The binary event payload containing event-specific data in JSON format.</param>
        /// <param name="eventId">The unique identifier for the event.</param>
        /// <param name="connection">The active database connection for executing queries.</param>
        /// <param name="transaction">The transaction to ensure atomicity of database operations. Should not be committed by the action.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Execute(BinaryData eventBody, Guid eventId, IDbConnection connection, IDbTransaction transaction);
    }
}
