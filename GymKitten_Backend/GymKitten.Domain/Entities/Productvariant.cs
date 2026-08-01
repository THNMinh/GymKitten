using System;
using System.Collections.Generic;

namespace GymKitten.Domain.Entities;

public partial class Productvariant
{
    public Guid Variantid { get; set; }

    public Guid Productid { get; set; }

    public string Sku { get; set; } = null!;

    public string Colorname { get; set; } = null!;

    public string? Colorhex { get; set; }

    public string Size { get; set; } = null!;

    public decimal Price { get; set; }

    public decimal? Originalprice { get; set; }

    public int? Weightgrams { get; set; }

    public DateTime Createdat { get; set; }

    public DateTime Updatedat { get; set; }

    public DateTime? Deletedat { get; set; }

    public virtual ICollection<Cartitem> Cartitems { get; set; } = new List<Cartitem>();

    public virtual Inventoryitem? Inventoryitem { get; set; }

    public virtual ICollection<Inventorytransaction> Inventorytransactions { get; set; } = new List<Inventorytransaction>();

    public virtual ICollection<Orderitem> Orderitems { get; set; } = new List<Orderitem>();

    public virtual Product Product { get; set; } = null!;

    public virtual ICollection<Productimage> Productimages { get; set; } = new List<Productimage>();
}
