namespace Storage
{
    public enum EventType
    {
        MeetingStarted = 0, //do not change int values!
        MeetingEnded = 1,
        VotingStarted = 2,
        VotingEnded = 3,
        Statements = 4,
        Attendees = 5,
        Case = 6,
        RollCallStarted = 7,
        RollCallEnded = 8,
        StatementReservation = 9,
        StatementReservationsCleared = 10,
        StatementStarted = 11,
        StatementEnded = 12,
        PersonArrived = 13,
        PersonLeft = 14,
        Pause = 15,
        PauseInfo = 16,
        MeetingContinues = 17,
        DiscussionStarts = 18,
        SpeechTimer = 19,
        Propositions = 20,
        ReplyReservation = 21,
        ReplyReservationsCleared = 22,
    }
}
