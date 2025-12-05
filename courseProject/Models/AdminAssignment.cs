using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace courseProject.Models
{
    public class AdminAssignment
    {
        public int AdminAssignmentId { get; set; }
        public int? AdminUserId { get; set; }
        public string? AdminName { get; set; }
        public int RequestEntityId { get; set; }
        public DateTime AssignedAt { get; set; }

        // navigation
        public User? AdminUser { get; set; }
        public RequestEntity? RequestEntity { get; set; }
    }
}
