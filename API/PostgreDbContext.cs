using Microsoft.EntityFrameworkCore; 
using API.Models; 

public class PostgreDbContext : DbContext
{
    public DbSet<City> cities { get; set; }
    public DbSet<Line> lines { get; set; }
    
    public PostgreDbContext(DbContextOptions<PostgreDbContext> options) : base(options)
    {
    }
}