namespace Storage
{
    /// <summary>
    /// Defines all types of events that can occur during a meeting.
    /// Each event type corresponds to specific actions that process the event data.
    /// Integer values must not be changed as they are persisted in the database.
    /// </summary>
    public enum EventType
    {
        /// <summary>Meeting has started.</summary>
        MeetingStarted = 0,
        /// <summary>Meeting has ended.</summary>
        MeetingEnded = 1,
        /// <summary>A voting session has started.</summary>
        VotingStarted = 2,
        /// <summary>A voting session has ended with results.</summary>
        VotingEnded = 3,
        /// <summary>Batch update of statement records.</summary>
        Statements = 4,
        /// <summary>Update to meeting seat assignments.</summary>
        Attendees = 5,
        /// <summary>Case or agenda item information update.</summary>
        Case = 6,
        /// <summary>Roll call attendance verification has started.</summary>
        RollCallStarted = 7,
        /// <summary>Roll call attendance verification has ended.</summary>
        RollCallEnded = 8,
        /// <summary>A participant has requested to make a statement.</summary>
        StatementReservation = 9,
        /// <summary>All statement reservations have been cleared.</summary>
        StatementReservationsCleared = 10,
        /// <summary>A participant has begun making their statement.</summary>
        StatementStarted = 11,
        /// <summary>A participant has finished their statement.</summary>
        StatementEnded = 12,
        /// <summary>A participant has arrived at the meeting.</summary>
        PersonArrived = 13,
        /// <summary>A participant has left the meeting.</summary>
        PersonLeft = 14,
        /// <summary>The meeting has been paused.</summary>
        Pause = 15,
        /// <summary>Information about a meeting pause or break.</summary>
        PauseInfo = 16,
        /// <summary>The meeting continues after a pause.</summary>
        MeetingContinues = 17,
        /// <summary>Discussion on an agenda item has started.</summary>
        DiscussionStarts = 18,
        /// <summary>Speech timer event for managing speaking time.</summary>
        SpeechTimer = 19,
        /// <summary>Voting propositions have been submitted.</summary>
        Propositions = 20,
        /// <summary>A participant has requested to reply to a statement.</summary>
        ReplyReservation = 21,
        /// <summary>All reply reservations have been cleared.</summary>
        ReplyReservationsCleared = 22,
    }
}
