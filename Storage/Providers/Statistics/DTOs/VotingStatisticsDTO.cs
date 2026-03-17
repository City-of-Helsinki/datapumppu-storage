namespace Storage.Providers.Statistics.DTOs
{
    /// <summary>
    /// Data transfer object representing voting statistics aggregated by person.
    /// Contains vote counts across all voting types for a specific person.
    /// </summary>
    public class VotingStatisticsDTO
    {
        /// <summary>
        /// Gets or sets the name of the person.
        /// </summary>
        public string Person { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets additional information in Finnish (e.g., role or party).
        /// </summary>
        public string AdditionalInfoFi { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the total number of votes cast in favor.
        /// </summary>
        public int For { get; set; }

        /// <summary>
        /// Gets or sets the total number of votes cast against.
        /// </summary>
        public int Against { get; set; }

        /// <summary>
        /// Gets or sets the total number of empty (blank) votes.
        /// </summary>
        public int Empty { get; set; }

        /// <summary>
        /// Gets or sets the total number of absences during voting.
        /// </summary>
        public int Absent { get; set; }

        /// <summary>
        /// Gets or sets the sum of all votes (for + against + empty + absent).
        /// </summary>
        public int Sum { get; set; }
    }
}
