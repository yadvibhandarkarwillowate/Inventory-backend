using InventoryApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace InventoryApp.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // User table  
    public DbSet<User> Users { get  ; set ; }

    public DbSet<Product> Products { get; set; }
    
    public DbSet<InventoryTransaction> InventoryTransactions { get; set; }

    public DbSet<Supplier> Suppliers { get; set; }

    public DbSet<PurchaseOrder> PurchaseOrders { get; set; }

    public DbSet<PurchaseOrderItem> PurchaseOrderItems { get; set; }
    

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // user table
        modelBuilder.Entity<User>( entity => entity.ToTable("user")) ; 

        modelBuilder.Entity<InventoryTransaction>(entity =>
        {
            entity.ToTable("inventory_transactions");
            entity.Property(e => e.TransactionType).HasConversion<string>().HasMaxLength(20);
            entity.HasOne(e => e.Product).WithMany().HasForeignKey(e => e.ProductId);
        });

        modelBuilder.Entity<Product>(entity => entity.ToTable("products"));

        modelBuilder.Entity<Supplier>(entity => entity.ToTable("suppliers"));

        modelBuilder.Entity<PurchaseOrder>(entity =>
        {
            entity.ToTable("purchase_orders");
            entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(25);
            entity.Property(e => e.TotalAmount).HasPrecision(18, 2);
            entity.HasOne(e => e.Supplier).WithMany().HasForeignKey(e => e.SupplierId);
            entity.HasMany(e => e.Items).WithOne(i => i.PurchaseOrder).HasForeignKey(i => i.PurchaseOrderId);
        });

        modelBuilder.Entity<PurchaseOrderItem>(entity =>
        {
            entity.ToTable("purchase_order_items");
            entity.Property(e => e.PurchasePrice).HasPrecision(18, 2);
            entity.Property(e => e.Total).HasPrecision(18, 2);
            entity.HasOne(e => e.Product).WithMany().HasForeignKey(e => e.ProductId);
        });
    }
}