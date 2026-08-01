using System;
using System.Collections.Generic;

namespace GymKitten.Domain.Entities;

public partial class Refreshtoken
{
    public Guid Refreshtokenid { get; set; }

    public Guid Userid { get; set; }

    public string Token { get; set; } = null!;

    public string Jwtid { get; set; } = null!;

    public bool Isused { get; set; }

    public bool Isrevoked { get; set; }

    public DateTime Expirydate { get; set; }

    public DateTime Createdat { get; set; }

    public DateTime Updatedat { get; set; }

    public DateTime? Deletedat { get; set; }

    public virtual User User { get; set; } = null!;
}
