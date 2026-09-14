namespace Storage.Controllers.Event.DTOs
{
    /// <summary>
    /// Data transfer object for simple events that only require basic event information.
    /// Used for events like MeetingStarted, MeetingContinues, MeetingEnded, RollCallStarted,
    /// DiscussionStarts, Pause, StatementEnded, StatementReservationsCleared, and ReplyReservationsCleared.
    /// These events do not carry additional payload beyond the base EventDTO properties.
    /// </summary>
    public class SimpleEventDTO: EventDTO {}
}
