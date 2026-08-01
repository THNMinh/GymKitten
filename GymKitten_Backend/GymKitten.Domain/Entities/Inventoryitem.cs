using System;
using System.Collections.Generic;

namespace GymKitten.Domain.Entities;

public partial class Inventoryitem
{
    public Guid Inventoryid { get; set; }

    public Guid Variantid { get; set; }

    public int Quantityonhand { get; set; }

    public int Quantityreserved { get; set; }

    public int Safetystock { get; set; }

    public int Rowversion { get; set; }

    public DateTime Createdat { get; set; }

    public DateTime Updatedat { get; set; }

    public DateTime? Deletedat { get; set; }

    public virtual Productvariant Variant { get; set; } = null!;
}
