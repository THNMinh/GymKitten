using System;
using System.Collections.Generic;

namespace GymKitten.Domain.Entities;

public partial class Reviewmedia
{
    public Guid Mediaid { get; set; }

    public Guid Reviewid { get; set; }

    public string Mediaurl { get; set; } = null!;

    public string Mediatype { get; set; } = null!;

    public DateTime Createdat { get; set; }

    public DateTime Updatedat { get; set; }

    public DateTime? Deletedat { get; set; }

    public virtual Productreview Review { get; set; } = null!;
}
