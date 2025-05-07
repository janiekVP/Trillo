namespace CardService.Models
{
    public class Card
    {
        public int Id { get; set; }
        public int BoardId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public int Priority { get; set; }
        public DateTime Due_date { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
