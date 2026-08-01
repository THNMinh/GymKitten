using System;
using System.Collections.Generic;

namespace GymKitten.Domain.Entities;

public partial class Cartitem
{
    public Guid Cartitemid { get; set; }

    public Guid Cartid { get; set; }

    public Guid Variantid { get; set; }

    public int Quantity { get; set; }

    public DateTime Createdat { get; set; }

    public DateTime Updatedat { get; set; }

    public DateTime? Deletedat { get; set; }

    public virtual Cart Cart { get; set; } = null!;

    public virtual Productvariant Variant { get; set; } = null!;
}
