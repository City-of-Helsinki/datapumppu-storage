namespace Storage
{
    /// <summary>
    /// Defines the possible vote values a participant can cast.
    /// Integer values must not be changed as they are persisted in the database.
    /// </summary>
    public enum VoteType
    {
        /// <summary>Vote in favor of the proposition.</summary>
        For = 0,
        /// <summary>Vote against the proposition.</summary>
        Against = 1,
        /// <summary>Abstain from voting (empty vote).</summary>
        Empty = 2,
        /// <summary>Participant was absent and did not vote.</summary>
        Absent = 3
    }
}
