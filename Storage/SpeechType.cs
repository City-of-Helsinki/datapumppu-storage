namespace Storage
{
    /// <summary>
    /// Classifies the type of speech made during a meeting.
    /// Integer values must not be changed as they are persisted in the database.
    /// </summary>
    public enum SpeechType
    {
        /// <summary>A reply to another participant's statement.</summary>
        Reply = 0,
        /// <summary>A primary statement on an agenda item.</summary>
        Statement = 1,
    }
}
