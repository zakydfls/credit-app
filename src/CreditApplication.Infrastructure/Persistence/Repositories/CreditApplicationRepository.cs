using Microsoft.EntityFrameworkCore;
using CreditApplication.Application.Abstractions.Persistence;
using CreditApplication.Application.Common;
using CreditApplicationEntity = CreditApplication.Domain.Entities.CreditApplication;

namespace CreditApplication.Infrastructure.Persistence.Repositories;

public sealed class CreditApplicationRepository(
    CreditApplicationDbContext dbContext) : ICreditApplicationRepository
{
    public Task<CreditApplicationEntity?> GetByIdAsync(
        Guid id,
        Guid userId,
        CancellationToken cancellationToken) =>
        dbContext.CreditApplications.SingleOrDefaultAsync(
            application => application.Id == id && application.UserId == userId,
            cancellationToken);

    public async Task<PagedResult<CreditApplicationEntity>> ListAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var query = dbContext.CreditApplications.AsNoTracking()
            .Where(application => application.UserId == userId);
        var totalCount = await query.CountAsync(cancellationToken);
        var offset = ((long)page - 1) * pageSize;
        if (offset >= totalCount)
            return new PagedResult<CreditApplicationEntity>([], page, pageSize, totalCount);

        var items = await query
            .OrderByDescending(application => application.CreatedAt)
            .ThenBy(application => application.Id)
            .Skip((int)offset)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<CreditApplicationEntity>(
            items,
            page,
            pageSize,
            totalCount);
    }

    public void Add(CreditApplicationEntity creditApplication) =>
        dbContext.CreditApplications.Add(creditApplication);

    public void Remove(CreditApplicationEntity creditApplication) =>
        dbContext.CreditApplications.Remove(creditApplication);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
