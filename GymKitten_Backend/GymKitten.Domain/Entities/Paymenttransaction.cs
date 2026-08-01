using System;
using System.Collections.Generic;

namespace GymKitten.Domain.Entities;

public partial class Paymenttransaction
{
    public Guid Transactionid { get; set; }

    public Guid Orderid { get; set; }

    public string Gateway { get; set; } = null!;

    public string? Gatewaytransactionid { get; set; }

    public decimal Amount { get; set; }

    public string Status { get; set; } = null!;

    public DateTime? Paymentdate { get; set; }

    public DateTime Createdat { get; set; }

    public DateTime Updatedat { get; set; }

    public DateTime? Deletedat { get; set; }

    public virtual Order Order { get; set; } = null!;
}
