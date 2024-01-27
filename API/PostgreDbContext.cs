using Microsoft.EntityFrameworkCore; 
using API.Models; 

public class PostgreDbContext : DbContext
{
    public DbSet<City> cities { get; set; }
    public DbSet<API.Models.Route> routes { get; set; }
    
    public PostgreDbContext(DbContextOptions<PostgreDbContext> options) : base(options)
    {
    }
}