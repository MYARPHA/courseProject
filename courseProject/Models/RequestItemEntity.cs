using System;

namespace courseProject.Models
{
    public class RequestItemEntity
    {
        public int RequestItemEntityId { get; set; }
        public int RequestEntityId { get; set; }
        public string? Title { get; set; }
        public decimal Price { get; set; }

        public virtual RequestEntity? Request { get; set; }
    }
}
