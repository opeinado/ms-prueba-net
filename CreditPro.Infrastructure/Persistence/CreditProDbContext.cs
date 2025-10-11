using CreditPro.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CreditPro.Infrastructure.Persistence;

public class CreditProDbContext : DbContext
{
    public CreditProDbContext(DbContextOptions<CreditProDbContext> options)
        : base(options)
    {
    }

    public DbSet<CreditApplication> CreditApplications => Set<CreditApplication>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CreditProDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
