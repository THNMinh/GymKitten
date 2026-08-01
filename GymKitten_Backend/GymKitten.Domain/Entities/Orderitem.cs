using System;
using System.Collections.Generic;

namespace GymKitten.Domain.Entities;

public partial class Orderitem
{
    public Guid Orderitemid { get; set; }

    public Guid Orderid { get; set; }

    public Guid Variantid { get; set; }

    public string Sku { get; set; } = null!;

    public string Productname { get; set; } = null!;

    public decimal Unitprice { get; set; }

    public int Quantity { get; set; }

    public decimal Totalprice { get; set; }

    public DateTime Createdat { get; set; }

    public DateTime Updatedat { get; set; }

    public DateTime? Deletedat { get; set; }

    public virtual Order Order { get; set; } = null!;

    public virtual Productvariant Variant { get; set; } = null!;
}
