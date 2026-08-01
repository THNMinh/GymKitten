using System;
using System.Collections.Generic;

namespace GymKitten.Domain.Entities;

public partial class Systemlog
{
    public Guid Logid { get; set; }

    public Guid? Userid { get; set; }

    public string Loglevel { get; set; } = null!;

    public string Action { get; set; } = null!;

    public string Message { get; set; } = null!;

    public string? Ipaddress { get; set; }

    public string? Useragent { get; set; }

    public DateTime Createdat { get; set; }

    public DateTime Updatedat { get; set; }

    public DateTime? Deletedat { get; set; }
}
