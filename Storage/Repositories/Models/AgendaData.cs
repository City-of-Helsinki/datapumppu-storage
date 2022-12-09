namespace Storage.Repositories.Models
{
    public class AgendaData
    {
        public List<AgendaItem> Existing { get; set; }

        public List<AgendaItem> NonExisting { get; set; }
    }
}
