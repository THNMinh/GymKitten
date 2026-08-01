using System;
using System.Collections.Generic;

namespace GymKitten.Domain.Entities;

public partial class Product
{
    public Guid Productid { get; set; }

    public Guid Categoryid { get; set; }

    public string Name { get; set; } = null!;

    public string Slug { get; set; } = null!;

    public string? Description { get; set; }

    public string? Fittype { get; set; }

    public string Gender { get; set; } = null!;

    public bool Isactive { get; set; }

    public DateTime Createdat { get; set; }

    public DateTime Updatedat { get; set; }

    public DateTime? Deletedat { get; set; }

    public virtual Category Category { get; set; } = null!;

    public virtual ICollection<Productimage> Productimages { get; set; } = new List<Productimage>();

    public virtual ICollection<Productreview> Productreviews { get; set; } = new List<Productreview>();

    public virtual ICollection<Productvariant> Productvariants { get; set; } = new List<Productvariant>();

    public virtual ICollection<Sizeguide> Sizeguides { get; set; } = new List<Sizeguide>();

    public virtual ICollection<Wishlist> Wishlists { get; set; } = new List<Wishlist>();
}
