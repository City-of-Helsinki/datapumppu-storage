namespace Storage
{
    public enum EventType
    {
        MeetingStarted = 0, //do not change int values!
        MeetingEnded = 1,
        VotingStarted = 2,
        VotingEnded = 3,
        SpeakingTurns = 4,
        Attendees = 5,
        Case = 6,
        RollCallStarted = 7,
        RollCallEnded = 8,
        SpeakingTurnReservation = 9,
        SpeakingTurnReservationsEmptied = 10,
        SpeakingTurnStarted = 11,
        SpeakingTurnEnded = 12,
        PersonArrived = 13,
        PersonLeft = 14,
        Break = 15,
        BreakNotice = 16,
        MeetingContinues = 17,
        DiscussionStarts = 18,
        SpeechTimer = 19,
        Propositions = 20,
        ReplyReservation = 21,
        ReplyReservationsEmptied = 22,
    }
}
