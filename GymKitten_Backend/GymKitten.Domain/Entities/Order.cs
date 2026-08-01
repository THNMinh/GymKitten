using System;
using System.Collections.Generic;

namespace GymKitten.Domain.Entities;

public partial class Order
{
    public Guid Orderid { get; set; }

    public string Ordercode { get; set; } = null!;

    public Guid? Userid { get; set; }

    public string Shippingaddress { get; set; } = null!;

    public decimal Subtotal { get; set; }

    public decimal Shippingfee { get; set; }

    public decimal Discountamount { get; set; }

    public decimal Totalamount { get; set; }

    public string Currentstatus { get; set; } = null!;

    public string Paymentmethod { get; set; } = null!;

    public string Paymentstatus { get; set; } = null!;

    public string? Customernote { get; set; }

    public DateTime Createdat { get; set; }

    public DateTime Updatedat { get; set; }

    public DateTime? Deletedat { get; set; }

    public virtual ICollection<Couponusage> Couponusages { get; set; } = new List<Couponusage>();

    public virtual ICollection<Orderitem> Orderitems { get; set; } = new List<Orderitem>();

    public virtual ICollection<Ordertrackinghistory> Ordertrackinghistories { get; set; } = new List<Ordertrackinghistory>();

    public virtual ICollection<Paymenttransaction> Paymenttransactions { get; set; } = new List<Paymenttransaction>();

    public virtual ICollection<Productreview> Productreviews { get; set; } = new List<Productreview>();

    public virtual User? User { get; set; }
}
