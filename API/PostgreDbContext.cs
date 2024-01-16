using Microsoft.EntityFrameworkCore; 

public class PostgreDbContext : DbContext
{
    public DbSet<City> Cities { get; set; }
    public DbSet<Route> Routes { get; set; }
    public DbSet<CityRoute> CityRoutes { get; set; }

    public PostgreDbContext(DbContextOptions<PostgreDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CityRoute>()
            .HasKey(cr => new { cr.CityId, cr.RouteId });

        modelBuilder.Entity<CityRoute>()
            .HasOne(cr => cr.City)
            .WithMany(c => c.CityRoutes)
            .HasForeignKey(cr => cr.CityId);

        modelBuilder.Entity<CityRoute>()
            .HasOne(cr => cr.Route)
            .WithMany(r => r.CityRoutes)
            .HasForeignKey(cr => cr.RouteId);
    }
}
