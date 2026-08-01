using System;
using System.Collections.Generic;

namespace GymKitten.Domain.Entities;

public partial class Couponusage
{
    public Guid Usageid { get; set; }

    public Guid Couponid { get; set; }

    public Guid Userid { get; set; }

    public Guid Orderid { get; set; }

    public DateTime Usedat { get; set; }

    public DateTime Createdat { get; set; }

    public DateTime Updatedat { get; set; }

    public DateTime? Deletedat { get; set; }

    public virtual Coupon Coupon { get; set; } = null!;

    public virtual Order Order { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
