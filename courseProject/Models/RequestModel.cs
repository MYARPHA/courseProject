namespace courseProject.Models
{
    public class RequestItem
    {
        public int ServiceId { get; set; }
        public string? Title { get; set; }
        public decimal Price { get; set; }
    }

    public class RequestModel
    {
        public int Id { get; set; }
        public string CustomerName { get; set; } = "";
        public string CustomerEmail { get; set; } = "";
        public string? CustomerPhone { get; set; }
        public List<RequestItem> Items { get; set; } = new();
        public decimal Total { get; set; }
        public string Status { get; set; } = "Новая";
        public string? AssignedTo { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
