using Microsoft.EntityFrameworkCore;
using ProcesoETL.Models;

namespace ProcesoETL.Data;

/// <summary>
/// Database context for source database (reviews extraction)
/// </summary>
public class SourceDbContext : DbContext
{
    public SourceDbContext(DbContextOptions<SourceDbContext> options) : base(options)
    {
    }

    public DbSet<Review> Reviews { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}

