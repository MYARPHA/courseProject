using System.Collections.Generic;

namespace courseProject.Models
{
    public class ProfileViewModel
    {
        public User User { get; set; } = null!;
        public IEnumerable<RequestModel> Requests { get; set; } = new List<RequestModel>();
    }
}
