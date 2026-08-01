using System;
using System.Collections.Generic;

namespace GymKitten.Domain.Entities;

public partial class Productreview
{
    public Guid Reviewid { get; set; }

    public Guid Productid { get; set; }

    public Guid Userid { get; set; }

    public Guid Orderid { get; set; }

    public int Rating { get; set; }

    public string? Comment { get; set; }

    public string? Fitfeedback { get; set; }

    public DateTime Createdat { get; set; }

    public DateTime Updatedat { get; set; }

    public DateTime? Deletedat { get; set; }

    public virtual Order Order { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;

    public virtual ICollection<Reviewmedia> Reviewmedia { get; set; } = new List<Reviewmedia>();

    public virtual User User { get; set; } = null!;
}
