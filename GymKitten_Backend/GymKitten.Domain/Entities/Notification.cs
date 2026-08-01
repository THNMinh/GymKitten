using System;
using System.Collections.Generic;

namespace GymKitten.Domain.Entities;

public partial class Notification
{
    public Guid Notificationid { get; set; }

    public Guid Userid { get; set; }

    public string Title { get; set; } = null!;

    public string Content { get; set; } = null!;

    public string Type { get; set; } = null!;

    public bool Isread { get; set; }

    public string? Targeturl { get; set; }

    public DateTime? Readat { get; set; }

    public DateTime Createdat { get; set; }

    public DateTime Updatedat { get; set; }

    public DateTime? Deletedat { get; set; }

    public virtual User User { get; set; } = null!;
}
