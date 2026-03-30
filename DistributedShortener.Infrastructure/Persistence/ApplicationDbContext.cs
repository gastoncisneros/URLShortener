using DistributedShortener.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;

namespace DistributedShortener.Infrastructure.Persistence;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<ShortLink> ShortLinks =>  Set<ShortLink>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Scans the assembly and applies every configuration that implements
        // IEntityTypeConfiguration<T> like ShortLinkConfiguration
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}