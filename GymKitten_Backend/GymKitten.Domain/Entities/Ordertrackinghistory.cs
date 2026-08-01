using System;
using System.Collections.Generic;

namespace GymKitten.Domain.Entities;

public partial class Ordertrackinghistory
{
    public Guid Trackingid { get; set; }

    public Guid Orderid { get; set; }

    public string Status { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string? Location { get; set; }

    public DateTime Timestamp { get; set; }

    public string? Updatedby { get; set; }

    public DateTime Createdat { get; set; }

    public DateTime Updatedat { get; set; }

    public DateTime? Deletedat { get; set; }

    public virtual Order Order { get; set; } = null!;
}
