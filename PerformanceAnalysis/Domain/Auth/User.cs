using System;
using System.Collections.Generic;

namespace Domain.Auth;

public partial class User
{
    public int Id { get; set; }

    public string Login { get; set; } = null!;

    public string Passwordhash { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Firstname { get; set; } = null!;

    public string? Middlename { get; set; }

    public string Lastname { get; set; } = null!;

    public string Role { get; set; } = null!;

    public DateTime Createdat { get; set; }

    public virtual ICollection<Refreshtoken> Refreshtokens { get; set; } = new List<Refreshtoken>();

    public virtual Student? Student { get; set; }
}
