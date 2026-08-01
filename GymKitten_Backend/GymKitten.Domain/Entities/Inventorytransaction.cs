using System;
using System.Collections.Generic;

namespace GymKitten.Domain.Entities;

public partial class Inventorytransaction
{
    public Guid Transactionid { get; set; }

    public Guid Variantid { get; set; }

    public int Quantitychange { get; set; }

    public string Type { get; set; } = null!;

    public string? Referenceid { get; set; }

    public DateTime Createdat { get; set; }

    public DateTime Updatedat { get; set; }

    public DateTime? Deletedat { get; set; }

    public virtual Productvariant Variant { get; set; } = null!;
}
