using System;
using System.Collections.Generic;

namespace Domain.Auth;

public partial class Refreshtoken
{
    public int Id { get; set; }

    public string Tokenhash { get; set; } = null!;

    public int Userid { get; set; }

    public DateTime Createdat { get; set; }

    public DateTime Expiresat { get; set; }

    public DateTime? Revokedat { get; set; }

    public virtual User User { get; set; } = null!;
}
