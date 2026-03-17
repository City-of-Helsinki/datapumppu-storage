namespace Storage
{
    /// <summary>
    /// Defines the type of voting procedure used in a meeting.
    /// Integer values must not be changed as they are persisted in the database.
    /// </summary>
    public enum VotingType
    {
        /// <summary>Normal voting procedure.</summary>
        Normal = 0,
        /// <summary>Pon - Finnish voting procedure type.</summary>
        Pon = 1,
        /// <summary>Pal - Finnish voting procedure type (return to committee).</summary>
        Pal = 2,
        /// <summary>Hyl - Finnish voting procedure type (reject).</summary>
        Hyl = 3,
        /// <summary>Vas - Finnish voting procedure type (counter-proposition).</summary>
        Vas = 4,
        /// <summary>Ppa - Finnish voting procedure type (table/defer).</summary>
        Ppa = 5,
    }
}
