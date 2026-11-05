using Microsoft.EntityFrameworkCore;
using CreditApplicationEntity = CreditApplication.Domain.Entities.CreditApplication;
using CreditApplication.Domain.Entities;

namespace CreditApplication.Infrastructure.Persistence;

public sealed class CreditApplicationDbContext(
    DbContextOptions<CreditApplicationDbContext> options) : DbContext(options)
{
    public DbSet<CreditApplicationEntity> CreditApplications =>
        Set<CreditApplicationEntity>();

    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(CreditApplicationDbContext).Assembly);
    }
}
