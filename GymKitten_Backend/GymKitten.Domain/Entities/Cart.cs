using System;
using System.Collections.Generic;

namespace GymKitten.Domain.Entities;

public partial class Cart
{
    public Guid Cartid { get; set; }

    public Guid? Userid { get; set; }

    public string? Sessionid { get; set; }

    public DateTime Createdat { get; set; }

    public DateTime Updatedat { get; set; }

    public DateTime? Deletedat { get; set; }

    public virtual ICollection<Cartitem> Cartitems { get; set; } = new List<Cartitem>();

    public virtual User? User { get; set; }
}
