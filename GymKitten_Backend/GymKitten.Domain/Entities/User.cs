using System;
using System.Collections.Generic;

namespace GymKitten.Domain.Entities;

public partial class User
{
    public Guid Userid { get; set; }

    public string Email { get; set; } = null!;

    public string? Passwordhash { get; set; }

    public string? Fullname { get; set; }

    public string? Phone { get; set; }

    public string? Avatarurl { get; set; }

    public string Role { get; set; } = null!;

    public string? Identityid { get; set; }

    public bool Isemailverified { get; set; }

    public bool Isactive { get; set; }

    public string? Fcmtoken { get; set; }

    public DateTime Createdat { get; set; }

    public DateTime Updatedat { get; set; }

    public DateTime? Deletedat { get; set; }

    public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();

    public virtual ICollection<Couponusage> Couponusages { get; set; } = new List<Couponusage>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<Productreview> Productreviews { get; set; } = new List<Productreview>();

    public virtual ICollection<Refreshtoken> Refreshtokens { get; set; } = new List<Refreshtoken>();

    public virtual ICollection<Useraddress> Useraddresses { get; set; } = new List<Useraddress>();

    public virtual ICollection<Wishlist> Wishlists { get; set; } = new List<Wishlist>();
}
