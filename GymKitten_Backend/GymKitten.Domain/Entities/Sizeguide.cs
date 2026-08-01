using System;
using System.Collections.Generic;

namespace GymKitten.Domain.Entities;

public partial class Sizeguide
{
    public Guid Guideid { get; set; }

    public Guid Productid { get; set; }

    public string Size { get; set; } = null!;

    public string? Chestcm { get; set; }

    public string? Waistcm { get; set; }

    public string? Hipscm { get; set; }

    public string? Heightrangecm { get; set; }

    public DateTime Createdat { get; set; }

    public DateTime Updatedat { get; set; }

    public DateTime? Deletedat { get; set; }

    public virtual Product Product { get; set; } = null!;
}
