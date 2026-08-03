using System;
using System.Collections.Generic;

namespace GymKitten.Domain.Entities;

public partial class Productimage
{
    public Guid Imageid { get; set; }

    public Guid Productid { get; set; }

    public Guid? Variantid { get; set; }

    public string Imageurl { get; set; } = null!;

    public int Displayorder { get; set; }

    public bool Isprimary { get; set; }

    public DateTime Createdat { get; set; }

    public DateTime Updatedat { get; set; }

    public DateTime? Deletedat { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual Productvariant? Variant { get; set; }
}
