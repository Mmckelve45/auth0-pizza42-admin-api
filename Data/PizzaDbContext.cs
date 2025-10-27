using Microsoft.EntityFrameworkCore;
using PizzaAdminApi.Models;

namespace PizzaAdminApi.Data;

public class PizzaDbContext : DbContext
{
    public PizzaDbContext(DbContextOptions<PizzaDbContext> options) : base(options)
    {
    }

    public DbSet<Pizza> Pizzas { get; set; }
    public DbSet<Order> Orders { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Pizza entity
        modelBuilder.Entity<Pizza>(entity =>
        {
            entity.HasIndex(e => e.Name);
        });

        // Configure Order entity
        modelBuilder.Entity<Order>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever(); // String ID, not auto-generated
        });
    }
}
