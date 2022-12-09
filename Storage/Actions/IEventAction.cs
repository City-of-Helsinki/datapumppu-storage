using System.Data;

namespace Storage.Actions
{
    public interface IEventAction
    {

        public List<EventType>? EventTypes { get; }

        Task Execute(BinaryData eventBody, Guid eventId, IDbConnection connection, IDbTransaction transaction);
    }
}
