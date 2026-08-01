using System;
using System.Collections.Generic;

namespace GymKitten.Domain.Entities;

public partial class Category
{
    public Guid Categoryid { get; set; }

    public Guid? Parentcategoryid { get; set; }

    public string Name { get; set; } = null!;

    public string Slug { get; set; } = null!;

    public string? Description { get; set; }

    public int Displayorder { get; set; }

    public DateTime Createdat { get; set; }

    public DateTime Updatedat { get; set; }

    public DateTime? Deletedat { get; set; }

    public virtual ICollection<Category> InverseParentcategory { get; set; } = new List<Category>();

    public virtual Category? Parentcategory { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
