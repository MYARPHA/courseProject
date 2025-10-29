using System;
using System.Collections.Generic;

namespace courseProject.Models;

public partial class ServiceCategory
{
    public int CategoryId { get; set; }

    public string CategoryTitle { get; set; } = null!;

    public virtual ICollection<Service> Services { get; set; } = new List<Service>();
}
