using System;
using System.Collections.Generic;

namespace Domain.Auth;

public partial class Group
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int Directionid { get; set; }

    public int Courseid { get; set; }

    public int Projectid { get; set; }

    public virtual ICollection<Student> Students { get; set; } = new List<Student>();
}
