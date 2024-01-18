using Microsoft.EntityFrameworkCore; 
using API.Models; 

public class PostgreDbContext : DbContext
{
    public DbSet<City> cities { get; set; }
    public DbSet<API.Models.Route> routes { get; set; }
    public DbSet<CityRoute> cityroutes { get; set; }

    public PostgreDbContext(DbContextOptions<PostgreDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CityRoute>()
            .HasKey(cr => new { cr.city_id, cr.route_id });

        modelBuilder.Entity<API.Models.Route>()
            .HasOne(r => r.StartCity)
            .WithMany()
            .HasForeignKey(r => r.start_city_id)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<API.Models.Route>()
            .HasOne(r => r.EndCity)
            .WithMany()
            .HasForeignKey(r => r.end_city_id)
            .OnDelete(DeleteBehavior.Restrict);
    }
}