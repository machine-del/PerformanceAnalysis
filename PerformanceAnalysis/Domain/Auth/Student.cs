using System;
using System.Collections.Generic;

namespace Domain.Auth;

public partial class Student
{
    public int Id { get; set; }

    public string Phone { get; set; } = null!;

    public string Vkprofilelink { get; set; } = null!;

    public string? Avatarpath { get; set; }

    public int Userid { get; set; }

    public virtual User User { get; set; } = null!;

    public virtual ICollection<Group> Groups { get; set; } = new List<Group>();
}
