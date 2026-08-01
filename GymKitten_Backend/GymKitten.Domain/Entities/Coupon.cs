using System;
using System.Collections.Generic;

namespace GymKitten.Domain.Entities;

public partial class Coupon
{
    public Guid Couponid { get; set; }

    public string Code { get; set; } = null!;

    public string Discounttype { get; set; } = null!;

    public decimal Discountvalue { get; set; }

    public decimal Minordervalue { get; set; }

    public decimal? Maxdiscountamount { get; set; }

    public int? Usagelimit { get; set; }

    public int Usedcount { get; set; }

    public DateTime Startdate { get; set; }

    public DateTime Enddate { get; set; }

    public bool Isactive { get; set; }

    public DateTime Createdat { get; set; }

    public DateTime Updatedat { get; set; }

    public DateTime? Deletedat { get; set; }

    public virtual ICollection<Couponusage> Couponusages { get; set; } = new List<Couponusage>();
}
