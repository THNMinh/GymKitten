using System;
using System.Collections.Generic;

namespace GymKitten.Domain.Entities;

public partial class Useraddress
{
    public Guid Addressid { get; set; }

    public Guid Userid { get; set; }

    public string Receivername { get; set; } = null!;

    public string Phonenumber { get; set; } = null!;

    public string Addressline1 { get; set; } = null!;

    public string Ward { get; set; } = null!;

    public string District { get; set; } = null!;

    public string City { get; set; } = null!;

    public bool Isdefault { get; set; }

    public string Addresstype { get; set; } = null!;

    public DateTime Createdat { get; set; }

    public DateTime Updatedat { get; set; }

    public DateTime? Deletedat { get; set; }

    public virtual User User { get; set; } = null!;
}
