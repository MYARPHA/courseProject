using System;
using System.Collections.Generic;

namespace courseProject.Models;

public partial class Contract
{
    public int ContractId { get; set; }

    public int ClientId { get; set; }

    public int UserId { get; set; }

    public virtual Client Client { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
