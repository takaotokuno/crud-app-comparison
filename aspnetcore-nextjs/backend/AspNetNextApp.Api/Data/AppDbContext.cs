using AspNetNextApp.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace AspNetNextApp.Api.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    public DbSet<Product> Products => Set<Product>();

    public DbSet<Stock> Stocks => Set<Stock>();

    public DbSet<StockTransaction> StockTransactions => Set<StockTransaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(user => user.Email).IsUnique();
            entity.Property(user => user.Email).HasMaxLength(255).IsRequired();
            entity.Property(user => user.PasswordHash).HasMaxLength(512).IsRequired();
            entity.Property(user => user.Name).HasMaxLength(100).IsRequired();
            entity.Property(user => user.Role).HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.Property(user => user.CreatedAt).IsRequired();
            entity.Property(user => user.UpdatedAt).IsRequired();
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable(table => table.HasCheckConstraint("CK_Products_Price_NonNegative", "[Price] >= 0"));
            entity.HasIndex(product => product.Sku).IsUnique();
            entity.Property(product => product.Sku).HasMaxLength(32).IsRequired();
            entity.Property(product => product.Name).HasMaxLength(100).IsRequired();
            entity.Property(product => product.Description).HasMaxLength(1000);
            entity.Property(product => product.Category).HasMaxLength(50);
            entity.Property(product => product.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.Property(product => product.CreatedAt).IsRequired();
            entity.Property(product => product.UpdatedAt).IsRequired();

            entity.HasOne(product => product.Stock)
                .WithOne(stock => stock.Product)
                .HasForeignKey<Stock>(stock => stock.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Stock>(entity =>
        {
            entity.ToTable(table =>
            {
                table.HasCheckConstraint("CK_Stocks_Quantity_NonNegative", "[Quantity] >= 0");
                table.HasCheckConstraint("CK_Stocks_SafetyStock_NonNegative", "[SafetyStock] >= 0");
            });
            entity.HasIndex(stock => stock.ProductId).IsUnique();
            entity.Property(stock => stock.CreatedAt).IsRequired();
            entity.Property(stock => stock.UpdatedAt).IsRequired();
        });

        modelBuilder.Entity<StockTransaction>(entity =>
        {
            entity.ToTable(table =>
            {
                table.HasCheckConstraint("CK_StockTransactions_QuantityDelta_NotZero", "[QuantityDelta] <> 0");
                table.HasCheckConstraint("CK_StockTransactions_QuantityAfter_NonNegative", "[QuantityAfter] >= 0");
            });
            entity.Property(transaction => transaction.Type).HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.Property(transaction => transaction.Reason).HasMaxLength(255);
            entity.Property(transaction => transaction.CreatedAt).IsRequired();

            entity.HasOne(transaction => transaction.Product)
                .WithMany(product => product.StockTransactions)
                .HasForeignKey(transaction => transaction.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(transaction => transaction.Stock)
                .WithMany(stock => stock.StockTransactions)
                .HasForeignKey(transaction => transaction.StockId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(transaction => transaction.CreatedBy)
                .WithMany(user => user.StockTransactions)
                .HasForeignKey(transaction => transaction.CreatedById)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }

    public override int SaveChanges()
    {
        SetTimestamps();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SetTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void SetTimestamps()
    {
        var now = DateTimeOffset.UtcNow;

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is User user)
            {
                SetAuditableTimestamps(entry.State, () => user.CreatedAt = now, () => user.UpdatedAt = now);
            }
            else if (entry.Entity is Product product)
            {
                SetAuditableTimestamps(entry.State, () => product.CreatedAt = now, () => product.UpdatedAt = now);
            }
            else if (entry.Entity is Stock stock)
            {
                SetAuditableTimestamps(entry.State, () => stock.CreatedAt = now, () => stock.UpdatedAt = now);
            }
            else if (entry.Entity is StockTransaction transaction && entry.State == EntityState.Added)
            {
                transaction.CreatedAt = now;
            }
        }
    }

    private static void SetAuditableTimestamps(EntityState state, Action setCreatedAt, Action setUpdatedAt)
    {
        if (state == EntityState.Added)
        {
            setCreatedAt();
            setUpdatedAt();
        }
        else if (state == EntityState.Modified)
        {
            setUpdatedAt();
        }
    }
}
