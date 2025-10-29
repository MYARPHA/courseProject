using System;
using System.Collections.Generic;

namespace courseProject.Models;

public partial class Service
{
    public int ServicesId { get; set; }

    public string ServicesTitle { get; set; } = null!;

    public int? CategoryId { get; set; }

    public decimal Price { get; set; }

    public string? Description { get; set; }

    public virtual ServiceCategory? Category { get; set; }
}
