using System;
using System.Collections.Generic;

namespace GymKitten.Domain.Entities;

public partial class Wishlist
{
    public Guid Wishlistid { get; set; }

    public Guid Userid { get; set; }

    public Guid Productid { get; set; }

    public DateTime Createdat { get; set; }

    public DateTime Updatedat { get; set; }

    public DateTime? Deletedat { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
