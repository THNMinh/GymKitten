using System;
using System.Collections.Generic;
using GymKitten.Application.Abstractions.Data;
using GymKitten.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymKitten.Infrastructure;

public partial class GymkittenContext : DbContext, IApplicationDbContext, IUnitOfWork
{
    private readonly IPublisher _publisher;

    public GymkittenContext()
    {
        _publisher = null!;
    }

    public GymkittenContext(DbContextOptions<GymkittenContext> options, IPublisher publisher)
        : base(options)
    {
        _publisher = publisher;
    }

    public virtual DbSet<Cart> Carts { get; set; }

    public virtual DbSet<Cartitem> Cartitems { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Coupon> Coupons { get; set; }

    public virtual DbSet<Couponusage> Couponusages { get; set; }

    public virtual DbSet<Inventoryitem> Inventoryitems { get; set; }

    public virtual DbSet<Inventorytransaction> Inventorytransactions { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<Orderitem> Orderitems { get; set; }

    public virtual DbSet<Ordertrackinghistory> Ordertrackinghistories { get; set; }

    public virtual DbSet<Paymenttransaction> Paymenttransactions { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<Productimage> Productimages { get; set; }

    public virtual DbSet<Productreview> Productreviews { get; set; }

    public virtual DbSet<Productvariant> Productvariants { get; set; }

    public virtual DbSet<Refreshtoken> Refreshtokens { get; set; }

    public virtual DbSet<Reviewmedia> Reviewmedias { get; set; }

    public virtual DbSet<Sizeguide> Sizeguides { get; set; }

    public virtual DbSet<Systemlog> Systemlogs { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Useraddress> Useraddresses { get; set; }

    public virtual DbSet<Wishlist> Wishlists { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Connection string is configured via DI in DependencyInjection.cs
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cart>(entity =>
        {
            entity.HasKey(e => e.Cartid).HasName("carts_pkey");

            entity.ToTable("carts", "order");

            entity.Property(e => e.Cartid)
                .ValueGeneratedNever()
                .HasColumnName("cartid");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("createdat");
            entity.Property(e => e.Deletedat).HasColumnName("deletedat");
            entity.Property(e => e.Sessionid)
                .HasMaxLength(100)
                .HasColumnName("sessionid");
            entity.Property(e => e.Updatedat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updatedat");
            entity.Property(e => e.Userid).HasColumnName("userid");

            entity.HasOne(d => d.User).WithMany(p => p.Carts)
                .HasForeignKey(d => d.Userid)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("carts_userid_fkey");
        });

        modelBuilder.Entity<Cartitem>(entity =>
        {
            entity.HasKey(e => e.Cartitemid).HasName("cartitems_pkey");

            entity.ToTable("cartitems", "order");

            entity.Property(e => e.Cartitemid)
                .ValueGeneratedNever()
                .HasColumnName("cartitemid");
            entity.Property(e => e.Cartid).HasColumnName("cartid");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("createdat");
            entity.Property(e => e.Deletedat).HasColumnName("deletedat");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.Updatedat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updatedat");
            entity.Property(e => e.Variantid).HasColumnName("variantid");

            entity.HasOne(d => d.Cart).WithMany(p => p.Cartitems)
                .HasForeignKey(d => d.Cartid)
                .HasConstraintName("cartitems_cartid_fkey");

            entity.HasOne(d => d.Variant).WithMany(p => p.Cartitems)
                .HasForeignKey(d => d.Variantid)
                .HasConstraintName("cartitems_variantid_fkey");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Categoryid).HasName("categories_pkey");

            entity.ToTable("categories", "catalog");

            entity.HasIndex(e => e.Slug, "categories_slug_key").IsUnique();

            entity.Property(e => e.Categoryid)
                .ValueGeneratedNever()
                .HasColumnName("categoryid");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("createdat");
            entity.Property(e => e.Deletedat).HasColumnName("deletedat");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Displayorder)
                .HasDefaultValue(0)
                .HasColumnName("displayorder");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.Parentcategoryid).HasColumnName("parentcategoryid");
            entity.Property(e => e.Slug)
                .HasMaxLength(150)
                .HasColumnName("slug");
            entity.Property(e => e.Updatedat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updatedat");

            entity.HasOne(d => d.Parentcategory).WithMany(p => p.InverseParentcategory)
                .HasForeignKey(d => d.Parentcategoryid)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("categories_parentcategoryid_fkey");
        });

        modelBuilder.Entity<Coupon>(entity =>
        {
            entity.HasKey(e => e.Couponid).HasName("coupons_pkey");

            entity.ToTable("coupons", "promotion");

            entity.HasIndex(e => e.Code, "coupons_code_key").IsUnique();

            entity.Property(e => e.Couponid)
                .ValueGeneratedNever()
                .HasColumnName("couponid");
            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .HasColumnName("code");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("createdat");
            entity.Property(e => e.Deletedat).HasColumnName("deletedat");
            entity.Property(e => e.Discounttype)
                .HasMaxLength(20)
                .HasColumnName("discounttype");
            entity.Property(e => e.Discountvalue)
                .HasPrecision(10, 2)
                .HasColumnName("discountvalue");
            entity.Property(e => e.Enddate).HasColumnName("enddate");
            entity.Property(e => e.Isactive)
                .HasDefaultValue(true)
                .HasColumnName("isactive");
            entity.Property(e => e.Maxdiscountamount)
                .HasPrecision(10, 2)
                .HasColumnName("maxdiscountamount");
            entity.Property(e => e.Minordervalue)
                .HasPrecision(10, 2)
                .HasColumnName("minordervalue");
            entity.Property(e => e.Startdate).HasColumnName("startdate");
            entity.Property(e => e.Updatedat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updatedat");
            entity.Property(e => e.Usagelimit).HasColumnName("usagelimit");
            entity.Property(e => e.Usedcount)
                .HasDefaultValue(0)
                .HasColumnName("usedcount");
        });

        modelBuilder.Entity<Couponusage>(entity =>
        {
            entity.HasKey(e => e.Usageid).HasName("couponusages_pkey");

            entity.ToTable("couponusages", "promotion");

            entity.Property(e => e.Usageid)
                .ValueGeneratedNever()
                .HasColumnName("usageid");
            entity.Property(e => e.Couponid).HasColumnName("couponid");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("createdat");
            entity.Property(e => e.Deletedat).HasColumnName("deletedat");
            entity.Property(e => e.Orderid).HasColumnName("orderid");
            entity.Property(e => e.Updatedat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updatedat");
            entity.Property(e => e.Usedat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("usedat");
            entity.Property(e => e.Userid).HasColumnName("userid");

            entity.HasOne(d => d.Coupon).WithMany(p => p.Couponusages)
                .HasForeignKey(d => d.Couponid)
                .HasConstraintName("couponusages_couponid_fkey");

            entity.HasOne(d => d.Order).WithMany(p => p.Couponusages)
                .HasForeignKey(d => d.Orderid)
                .HasConstraintName("couponusages_orderid_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.Couponusages)
                .HasForeignKey(d => d.Userid)
                .HasConstraintName("couponusages_userid_fkey");
        });

        modelBuilder.Entity<Inventoryitem>(entity =>
        {
            entity.HasKey(e => e.Inventoryid).HasName("inventoryitems_pkey");

            entity.ToTable("inventoryitems", "inventory");

            entity.HasIndex(e => e.Variantid, "idx_inventory_variant");

            entity.HasIndex(e => e.Variantid, "inventoryitems_variantid_key").IsUnique();

            entity.Property(e => e.Inventoryid)
                .ValueGeneratedNever()
                .HasColumnName("inventoryid");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("createdat");
            entity.Property(e => e.Deletedat).HasColumnName("deletedat");
            entity.Property(e => e.Quantityonhand)
                .HasDefaultValue(0)
                .HasColumnName("quantityonhand");
            entity.Property(e => e.Quantityreserved)
                .HasDefaultValue(0)
                .HasColumnName("quantityreserved");
            entity.Property(e => e.Rowversion)
                .HasDefaultValue(1)
                .HasColumnName("rowversion");
            entity.Property(e => e.Safetystock)
                .HasDefaultValue(5)
                .HasColumnName("safetystock");
            entity.Property(e => e.Updatedat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updatedat");
            entity.Property(e => e.Variantid).HasColumnName("variantid");

            entity.HasOne(d => d.Variant).WithOne(p => p.Inventoryitem)
                .HasForeignKey<Inventoryitem>(d => d.Variantid)
                .HasConstraintName("inventoryitems_variantid_fkey");
        });

        modelBuilder.Entity<Inventorytransaction>(entity =>
        {
            entity.HasKey(e => e.Transactionid).HasName("inventorytransactions_pkey");

            entity.ToTable("inventorytransactions", "inventory");

            entity.Property(e => e.Transactionid)
                .ValueGeneratedNever()
                .HasColumnName("transactionid");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("createdat");
            entity.Property(e => e.Deletedat).HasColumnName("deletedat");
            entity.Property(e => e.Quantitychange).HasColumnName("quantitychange");
            entity.Property(e => e.Referenceid)
                .HasMaxLength(100)
                .HasColumnName("referenceid");
            entity.Property(e => e.Type)
                .HasMaxLength(30)
                .HasColumnName("type");
            entity.Property(e => e.Updatedat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updatedat");
            entity.Property(e => e.Variantid).HasColumnName("variantid");

            entity.HasOne(d => d.Variant).WithMany(p => p.Inventorytransactions)
                .HasForeignKey(d => d.Variantid)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("inventorytransactions_variantid_fkey");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Notificationid).HasName("notifications_pkey");

            entity.ToTable("notifications", "system");

            entity.Property(e => e.Notificationid)
                .ValueGeneratedNever()
                .HasColumnName("notificationid");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("createdat");
            entity.Property(e => e.Deletedat).HasColumnName("deletedat");
            entity.Property(e => e.Isread)
                .HasDefaultValue(false)
                .HasColumnName("isread");
            entity.Property(e => e.Readat).HasColumnName("readat");
            entity.Property(e => e.Targeturl)
                .HasMaxLength(500)
                .HasColumnName("targeturl");
            entity.Property(e => e.Title)
                .HasMaxLength(200)
                .HasColumnName("title");
            entity.Property(e => e.Type)
                .HasMaxLength(50)
                .HasColumnName("type");
            entity.Property(e => e.Updatedat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updatedat");
            entity.Property(e => e.Userid).HasColumnName("userid");

            entity.HasOne(d => d.User).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.Userid)
                .HasConstraintName("notifications_userid_fkey");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Orderid).HasName("orders_pkey");

            entity.ToTable("orders", "order");

            entity.HasIndex(e => e.Ordercode, "idx_orders_code");

            entity.HasIndex(e => e.Userid, "idx_orders_user").HasFilter("(deletedat IS NULL)");

            entity.HasIndex(e => e.Ordercode, "orders_ordercode_key").IsUnique();

            entity.Property(e => e.Orderid)
                .ValueGeneratedNever()
                .HasColumnName("orderid");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("createdat");
            entity.Property(e => e.Currentstatus)
                .HasMaxLength(30)
                .HasColumnName("currentstatus");
            entity.Property(e => e.Customernote).HasColumnName("customernote");
            entity.Property(e => e.Deletedat).HasColumnName("deletedat");
            entity.Property(e => e.Discountamount)
                .HasPrecision(10, 2)
                .HasColumnName("discountamount");
            entity.Property(e => e.Ordercode)
                .HasMaxLength(30)
                .HasColumnName("ordercode");
            entity.Property(e => e.Paymentmethod)
                .HasMaxLength(20)
                .HasColumnName("paymentmethod");
            entity.Property(e => e.Paymentstatus)
                .HasMaxLength(20)
                .HasDefaultValueSql("'Unpaid'::character varying")
                .HasColumnName("paymentstatus");
            entity.Property(e => e.Shippingaddress).HasColumnName("shippingaddress");
            entity.Property(e => e.Shippingfee)
                .HasPrecision(10, 2)
                .HasColumnName("shippingfee");
            entity.Property(e => e.Subtotal)
                .HasPrecision(10, 2)
                .HasColumnName("subtotal");
            entity.Property(e => e.Totalamount)
                .HasPrecision(10, 2)
                .HasColumnName("totalamount");
            entity.Property(e => e.Updatedat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updatedat");
            entity.Property(e => e.Userid).HasColumnName("userid");

            entity.HasOne(d => d.User).WithMany(p => p.Orders)
                .HasForeignKey(d => d.Userid)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("orders_userid_fkey");
        });

        modelBuilder.Entity<Orderitem>(entity =>
        {
            entity.HasKey(e => e.Orderitemid).HasName("orderitems_pkey");

            entity.ToTable("orderitems", "order");

            entity.Property(e => e.Orderitemid)
                .ValueGeneratedNever()
                .HasColumnName("orderitemid");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("createdat");
            entity.Property(e => e.Deletedat).HasColumnName("deletedat");
            entity.Property(e => e.Orderid).HasColumnName("orderid");
            entity.Property(e => e.Productname)
                .HasMaxLength(255)
                .HasColumnName("productname");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.Sku)
                .HasMaxLength(50)
                .HasColumnName("sku");
            entity.Property(e => e.Totalprice)
                .HasPrecision(10, 2)
                .HasColumnName("totalprice");
            entity.Property(e => e.Unitprice)
                .HasPrecision(10, 2)
                .HasColumnName("unitprice");
            entity.Property(e => e.Updatedat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updatedat");
            entity.Property(e => e.Variantid).HasColumnName("variantid");

            entity.HasOne(d => d.Order).WithMany(p => p.Orderitems)
                .HasForeignKey(d => d.Orderid)
                .HasConstraintName("orderitems_orderid_fkey");

            entity.HasOne(d => d.Variant).WithMany(p => p.Orderitems)
                .HasForeignKey(d => d.Variantid)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("orderitems_variantid_fkey");
        });

        modelBuilder.Entity<Ordertrackinghistory>(entity =>
        {
            entity.HasKey(e => e.Trackingid).HasName("ordertrackinghistories_pkey");

            entity.ToTable("ordertrackinghistories", "order");

            entity.HasIndex(e => e.Orderid, "idx_tracking_order");

            entity.Property(e => e.Trackingid)
                .ValueGeneratedNever()
                .HasColumnName("trackingid");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("createdat");
            entity.Property(e => e.Deletedat).HasColumnName("deletedat");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Location)
                .HasMaxLength(150)
                .HasColumnName("location");
            entity.Property(e => e.Orderid).HasColumnName("orderid");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasColumnName("status");
            entity.Property(e => e.Timestamp).HasColumnName("timestamp");
            entity.Property(e => e.Title)
                .HasMaxLength(150)
                .HasColumnName("title");
            entity.Property(e => e.Updatedat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updatedat");
            entity.Property(e => e.Updatedby)
                .HasMaxLength(50)
                .HasColumnName("updatedby");

            entity.HasOne(d => d.Order).WithMany(p => p.Ordertrackinghistories)
                .HasForeignKey(d => d.Orderid)
                .HasConstraintName("ordertrackinghistories_orderid_fkey");
        });

        modelBuilder.Entity<Paymenttransaction>(entity =>
        {
            entity.HasKey(e => e.Transactionid).HasName("paymenttransactions_pkey");

            entity.ToTable("paymenttransactions", "payment");

            entity.Property(e => e.Transactionid)
                .ValueGeneratedNever()
                .HasColumnName("transactionid");
            entity.Property(e => e.Amount)
                .HasPrecision(10, 2)
                .HasColumnName("amount");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("createdat");
            entity.Property(e => e.Deletedat).HasColumnName("deletedat");
            entity.Property(e => e.Gateway)
                .HasMaxLength(50)
                .HasColumnName("gateway");
            entity.Property(e => e.Gatewaytransactionid)
                .HasMaxLength(100)
                .HasColumnName("gatewaytransactionid");
            entity.Property(e => e.Orderid).HasColumnName("orderid");
            entity.Property(e => e.Paymentdate).HasColumnName("paymentdate");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasColumnName("status");
            entity.Property(e => e.Updatedat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updatedat");

            entity.HasOne(d => d.Order).WithMany(p => p.Paymenttransactions)
                .HasForeignKey(d => d.Orderid)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("paymenttransactions_orderid_fkey");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Productid).HasName("products_pkey");

            entity.ToTable("products", "catalog");

            entity.HasIndex(e => e.Categoryid, "idx_products_category").HasFilter("(deletedat IS NULL)");

            entity.HasIndex(e => e.Slug, "products_slug_key").IsUnique();

            entity.Property(e => e.Productid)
                .ValueGeneratedNever()
                .HasColumnName("productid");
            entity.Property(e => e.Categoryid).HasColumnName("categoryid");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("createdat");
            entity.Property(e => e.Deletedat).HasColumnName("deletedat");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Fittype)
                .HasMaxLength(50)
                .HasColumnName("fittype");
            entity.Property(e => e.Gender)
                .HasMaxLength(20)
                .HasDefaultValueSql("'Unisex'::character varying")
                .HasColumnName("gender");
            entity.Property(e => e.Isactive)
                .HasDefaultValue(true)
                .HasColumnName("isactive");
            entity.Property(e => e.Name)
                .HasMaxLength(200)
                .HasColumnName("name");
            entity.Property(e => e.Slug)
                .HasMaxLength(250)
                .HasColumnName("slug");
            entity.Property(e => e.Updatedat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updatedat");

            entity.HasOne(d => d.Category).WithMany(p => p.Products)
                .HasForeignKey(d => d.Categoryid)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("products_categoryid_fkey");
        });

        modelBuilder.Entity<Productimage>(entity =>
        {
            entity.HasKey(e => e.Imageid).HasName("productimages_pkey");

            entity.ToTable("productimages", "catalog");

            entity.Property(e => e.Imageid)
                .ValueGeneratedNever()
                .HasColumnName("imageid");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("createdat");
            entity.Property(e => e.Deletedat).HasColumnName("deletedat");
            entity.Property(e => e.Displayorder)
                .HasDefaultValue(0)
                .HasColumnName("displayorder");
            entity.Property(e => e.Imageurl)
                .HasMaxLength(500)
                .HasColumnName("imageurl");
            entity.Property(e => e.Isprimary)
                .HasDefaultValue(false)
                .HasColumnName("isprimary");
            entity.Property(e => e.Productid).HasColumnName("productid");
            entity.Property(e => e.Updatedat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updatedat");
            entity.Property(e => e.Variantid).HasColumnName("variantid");

            entity.HasOne(d => d.Product).WithMany(p => p.Productimages)
                .HasForeignKey(d => d.Productid)
                .HasConstraintName("productimages_productid_fkey");

            entity.HasOne(d => d.Variant).WithMany(p => p.Productimages)
                .HasForeignKey(d => d.Variantid)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("productimages_variantid_fkey");
        });

        modelBuilder.Entity<Productreview>(entity =>
        {
            entity.HasKey(e => e.Reviewid).HasName("productreviews_pkey");

            entity.ToTable("productreviews", "social_proof");

            entity.Property(e => e.Reviewid)
                .ValueGeneratedNever()
                .HasColumnName("reviewid");
            entity.Property(e => e.Comment).HasColumnName("comment");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("createdat");
            entity.Property(e => e.Deletedat).HasColumnName("deletedat");
            entity.Property(e => e.Fitfeedback)
                .HasMaxLength(20)
                .HasColumnName("fitfeedback");
            entity.Property(e => e.Orderid).HasColumnName("orderid");
            entity.Property(e => e.Productid).HasColumnName("productid");
            entity.Property(e => e.Rating).HasColumnName("rating");
            entity.Property(e => e.Updatedat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updatedat");
            entity.Property(e => e.Userid).HasColumnName("userid");

            entity.HasOne(d => d.Order).WithMany(p => p.Productreviews)
                .HasForeignKey(d => d.Orderid)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("productreviews_orderid_fkey");

            entity.HasOne(d => d.Product).WithMany(p => p.Productreviews)
                .HasForeignKey(d => d.Productid)
                .HasConstraintName("productreviews_productid_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.Productreviews)
                .HasForeignKey(d => d.Userid)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("productreviews_userid_fkey");
        });

        modelBuilder.Entity<Productvariant>(entity =>
        {
            entity.HasKey(e => e.Variantid).HasName("productvariants_pkey");

            entity.ToTable("productvariants", "catalog");

            entity.HasIndex(e => e.Productid, "idx_variants_product").HasFilter("(deletedat IS NULL)");

            entity.HasIndex(e => e.Sku, "productvariants_sku_key").IsUnique();

            entity.Property(e => e.Variantid)
                .ValueGeneratedNever()
                .HasColumnName("variantid");
            entity.Property(e => e.Colorhex)
                .HasMaxLength(10)
                .HasColumnName("colorhex");
            entity.Property(e => e.Colorname)
                .HasMaxLength(50)
                .HasColumnName("colorname");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("createdat");
            entity.Property(e => e.Deletedat).HasColumnName("deletedat");
            entity.Property(e => e.Originalprice)
                .HasPrecision(10, 2)
                .HasColumnName("originalprice");
            entity.Property(e => e.Price)
                .HasPrecision(10, 2)
                .HasColumnName("price");
            entity.Property(e => e.Productid).HasColumnName("productid");
            entity.Property(e => e.Size)
                .HasMaxLength(10)
                .HasColumnName("size");
            entity.Property(e => e.Sku)
                .HasMaxLength(50)
                .HasColumnName("sku");
            entity.Property(e => e.Updatedat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updatedat");
            entity.Property(e => e.Weightgrams).HasColumnName("weightgrams");

            entity.HasOne(d => d.Product).WithMany(p => p.Productvariants)
                .HasForeignKey(d => d.Productid)
                .HasConstraintName("productvariants_productid_fkey");
        });

        modelBuilder.Entity<Refreshtoken>(entity =>
        {
            entity.HasKey(e => e.Refreshtokenid).HasName("refreshtokens_pkey");

            entity.ToTable("refreshtokens", "identity");

            entity.HasIndex(e => e.Token, "refreshtokens_token_key").IsUnique();

            entity.Property(e => e.Refreshtokenid)
                .ValueGeneratedNever()
                .HasColumnName("refreshtokenid");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("createdat");
            entity.Property(e => e.Deletedat).HasColumnName("deletedat");
            entity.Property(e => e.Expirydate).HasColumnName("expirydate");
            entity.Property(e => e.Isrevoked)
                .HasDefaultValue(false)
                .HasColumnName("isrevoked");
            entity.Property(e => e.Isused)
                .HasDefaultValue(false)
                .HasColumnName("isused");
            entity.Property(e => e.Jwtid)
                .HasMaxLength(255)
                .HasColumnName("jwtid");
            entity.Property(e => e.Token)
                .HasMaxLength(500)
                .HasColumnName("token");
            entity.Property(e => e.Updatedat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updatedat");
            entity.Property(e => e.Userid).HasColumnName("userid");

            entity.HasOne(d => d.User).WithMany(p => p.Refreshtokens)
                .HasForeignKey(d => d.Userid)
                .HasConstraintName("refreshtokens_userid_fkey");
        });

        modelBuilder.Entity<Reviewmedia>(entity =>
        {
            entity.HasKey(e => e.Mediaid).HasName("reviewmedias_pkey");

            entity.ToTable("reviewmedias", "social_proof");

            entity.Property(e => e.Mediaid)
                .ValueGeneratedNever()
                .HasColumnName("mediaid");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("createdat");
            entity.Property(e => e.Deletedat).HasColumnName("deletedat");
            entity.Property(e => e.Mediatype)
                .HasMaxLength(20)
                .HasDefaultValueSql("'Image'::character varying")
                .HasColumnName("mediatype");
            entity.Property(e => e.Mediaurl)
                .HasMaxLength(500)
                .HasColumnName("mediaurl");
            entity.Property(e => e.Reviewid).HasColumnName("reviewid");
            entity.Property(e => e.Updatedat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updatedat");

            entity.HasOne(d => d.Review).WithMany(p => p.Reviewmedia)
                .HasForeignKey(d => d.Reviewid)
                .HasConstraintName("reviewmedias_reviewid_fkey");
        });

        modelBuilder.Entity<Sizeguide>(entity =>
        {
            entity.HasKey(e => e.Guideid).HasName("sizeguides_pkey");

            entity.ToTable("sizeguides", "catalog");

            entity.Property(e => e.Guideid)
                .ValueGeneratedNever()
                .HasColumnName("guideid");
            entity.Property(e => e.Chestcm)
                .HasMaxLength(50)
                .HasColumnName("chestcm");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("createdat");
            entity.Property(e => e.Deletedat).HasColumnName("deletedat");
            entity.Property(e => e.Heightrangecm)
                .HasMaxLength(50)
                .HasColumnName("heightrangecm");
            entity.Property(e => e.Hipscm)
                .HasMaxLength(50)
                .HasColumnName("hipscm");
            entity.Property(e => e.Productid).HasColumnName("productid");
            entity.Property(e => e.Size)
                .HasMaxLength(10)
                .HasColumnName("size");
            entity.Property(e => e.Updatedat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updatedat");
            entity.Property(e => e.Waistcm)
                .HasMaxLength(50)
                .HasColumnName("waistcm");

            entity.HasOne(d => d.Product).WithMany(p => p.Sizeguides)
                .HasForeignKey(d => d.Productid)
                .HasConstraintName("sizeguides_productid_fkey");
        });

        modelBuilder.Entity<Systemlog>(entity =>
        {
            entity.HasKey(e => e.Logid).HasName("systemlogs_pkey");

            entity.ToTable("systemlogs", "system");

            entity.Property(e => e.Logid)
                .ValueGeneratedNever()
                .HasColumnName("logid");
            entity.Property(e => e.Action)
                .HasMaxLength(100)
                .HasColumnName("action");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("createdat");
            entity.Property(e => e.Deletedat).HasColumnName("deletedat");
            entity.Property(e => e.Ipaddress)
                .HasMaxLength(50)
                .HasColumnName("ipaddress");
            entity.Property(e => e.Loglevel)
                .HasMaxLength(20)
                .HasColumnName("loglevel");
            entity.Property(e => e.Message).HasColumnName("message");
            entity.Property(e => e.Updatedat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updatedat");
            entity.Property(e => e.Useragent).HasColumnName("useragent");
            entity.Property(e => e.Userid).HasColumnName("userid");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Userid).HasName("users_pkey");

            entity.ToTable("users", "identity");

            entity.HasIndex(e => e.Email, "users_email_key").IsUnique();

            entity.HasIndex(e => e.Identityid, "users_identityid_key").IsUnique();

            entity.Property(e => e.Userid)
                .ValueGeneratedNever()
                .HasColumnName("userid");
            entity.Property(e => e.Avatarurl)
                .HasMaxLength(500)
                .HasColumnName("avatarurl");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("createdat");
            entity.Property(e => e.Deletedat).HasColumnName("deletedat");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.Fcmtoken)
                .HasMaxLength(500)
                .HasColumnName("fcmtoken");
            entity.Property(e => e.Fullname)
                .HasMaxLength(100)
                .HasColumnName("fullname");
            entity.Property(e => e.Identityid)
                .HasMaxLength(255)
                .HasColumnName("identityid");
            entity.Property(e => e.Isactive)
                .HasDefaultValue(true)
                .HasColumnName("isactive");
            entity.Property(e => e.Isemailverified)
                .HasDefaultValue(false)
                .HasColumnName("isemailverified");
            entity.Property(e => e.Passwordhash)
                .HasMaxLength(255)
                .HasColumnName("passwordhash");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");
            entity.Property(e => e.Role)
                .HasMaxLength(20)
                .HasDefaultValueSql("'Customer'::character varying")
                .HasColumnName("role");
            entity.Property(e => e.Updatedat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updatedat");
        });

        modelBuilder.Entity<Useraddress>(entity =>
        {
            entity.HasKey(e => e.Addressid).HasName("useraddresses_pkey");

            entity.ToTable("useraddresses", "identity");

            entity.Property(e => e.Addressid)
                .ValueGeneratedNever()
                .HasColumnName("addressid");
            entity.Property(e => e.Addressline1)
                .HasMaxLength(255)
                .HasColumnName("addressline1");
            entity.Property(e => e.Addresstype)
                .HasMaxLength(20)
                .HasDefaultValueSql("'Home'::character varying")
                .HasColumnName("addresstype");
            entity.Property(e => e.City)
                .HasMaxLength(100)
                .HasColumnName("city");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("createdat");
            entity.Property(e => e.Deletedat).HasColumnName("deletedat");
            entity.Property(e => e.District)
                .HasMaxLength(100)
                .HasColumnName("district");
            entity.Property(e => e.Isdefault)
                .HasDefaultValue(false)
                .HasColumnName("isdefault");
            entity.Property(e => e.Phonenumber)
                .HasMaxLength(20)
                .HasColumnName("phonenumber");
            entity.Property(e => e.Receivername)
                .HasMaxLength(100)
                .HasColumnName("receivername");
            entity.Property(e => e.Updatedat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updatedat");
            entity.Property(e => e.Userid).HasColumnName("userid");
            entity.Property(e => e.Ward)
                .HasMaxLength(100)
                .HasColumnName("ward");

            entity.HasOne(d => d.User).WithMany(p => p.Useraddresses)
                .HasForeignKey(d => d.Userid)
                .HasConstraintName("useraddresses_userid_fkey");
        });

        modelBuilder.Entity<Wishlist>(entity =>
        {
            entity.HasKey(e => e.Wishlistid).HasName("wishlists_pkey");

            entity.ToTable("wishlists", "social_proof");

            entity.HasIndex(e => new { e.Userid, e.Productid }, "uq_wishlist_user_product").IsUnique();

            entity.Property(e => e.Wishlistid)
                .ValueGeneratedNever()
                .HasColumnName("wishlistid");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("createdat");
            entity.Property(e => e.Deletedat).HasColumnName("deletedat");
            entity.Property(e => e.Productid).HasColumnName("productid");
            entity.Property(e => e.Updatedat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updatedat");
            entity.Property(e => e.Userid).HasColumnName("userid");

            entity.HasOne(d => d.Product).WithMany(p => p.Wishlists)
                .HasForeignKey(d => d.Productid)
                .HasConstraintName("wishlists_productid_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.Wishlists)
                .HasForeignKey(d => d.Userid)
                .HasConstraintName("wishlists_userid_fkey");
        });

        // Global soft-delete query filters
        modelBuilder.Entity<Cart>().HasQueryFilter(e => e.Deletedat == null);
        modelBuilder.Entity<Cartitem>().HasQueryFilter(e => e.Deletedat == null);
        modelBuilder.Entity<Category>().HasQueryFilter(e => e.Deletedat == null);
        modelBuilder.Entity<Coupon>().HasQueryFilter(e => e.Deletedat == null);
        modelBuilder.Entity<Couponusage>().HasQueryFilter(e => e.Deletedat == null);
        modelBuilder.Entity<Inventoryitem>().HasQueryFilter(e => e.Deletedat == null);
        modelBuilder.Entity<Inventorytransaction>().HasQueryFilter(e => e.Deletedat == null);
        modelBuilder.Entity<Notification>().HasQueryFilter(e => e.Deletedat == null);
        modelBuilder.Entity<Order>().HasQueryFilter(e => e.Deletedat == null);
        modelBuilder.Entity<Orderitem>().HasQueryFilter(e => e.Deletedat == null);
        modelBuilder.Entity<Ordertrackinghistory>().HasQueryFilter(e => e.Deletedat == null);
        modelBuilder.Entity<Paymenttransaction>().HasQueryFilter(e => e.Deletedat == null);
        modelBuilder.Entity<Product>().HasQueryFilter(e => e.Deletedat == null);
        modelBuilder.Entity<Productimage>().HasQueryFilter(e => e.Deletedat == null);
        modelBuilder.Entity<Productreview>().HasQueryFilter(e => e.Deletedat == null);
        modelBuilder.Entity<Productvariant>().HasQueryFilter(e => e.Deletedat == null);
        modelBuilder.Entity<Refreshtoken>().HasQueryFilter(e => e.Deletedat == null);
        modelBuilder.Entity<Reviewmedia>().HasQueryFilter(e => e.Deletedat == null);
        modelBuilder.Entity<Sizeguide>().HasQueryFilter(e => e.Deletedat == null);
        modelBuilder.Entity<Systemlog>().HasQueryFilter(e => e.Deletedat == null);
        modelBuilder.Entity<User>().HasQueryFilter(e => e.Deletedat == null);
        modelBuilder.Entity<Useraddress>().HasQueryFilter(e => e.Deletedat == null);
        modelBuilder.Entity<Wishlist>().HasQueryFilter(e => e.Deletedat == null);

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Collect domain events from all tracked User entities
        var usersWithEvents = ChangeTracker
            .Entries<User>()
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => e.Entity)
            .ToList();

        var domainEvents = usersWithEvents
            .SelectMany(u => u.DomainEvents)
            .ToList();

        // Clear domain events before publishing to avoid re-entrancy
        foreach (var user in usersWithEvents)
        {
            user.ClearDomainEvents();
        }

        // Publish domain events via MediatR
        foreach (var domainEvent in domainEvents)
        {
            await _publisher.Publish(domainEvent, cancellationToken);
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
