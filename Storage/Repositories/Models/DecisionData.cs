namespace Storage.Repositories.Models
{
    public class DecisionData
    {
        public List<Decision> Existing { get; set; }

        public List<Decision> NonExisting { get; set; }
    }
}
