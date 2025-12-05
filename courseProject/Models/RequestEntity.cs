using System;
using System.Collections.Generic;

namespace courseProject.Models
{
    public class RequestEntity
    {
        public int RequestEntityId { get; set; }
        public string CustomerName { get; set; } = "";
        public string CustomerEmail { get; set; } = "";
        public string? CustomerPhone { get; set; }
        public string Status { get; set; } = "Новая";
        public string? AssignedTo { get; set; }
        public decimal Total { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual List<RequestItemEntity> Items { get; set; } = new();
    }
}
