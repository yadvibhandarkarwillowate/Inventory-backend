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
    }
}