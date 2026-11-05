using CreditApplication.Application.Common;
using CreditApplicationEntity = CreditApplication.Domain.Entities.CreditApplication;

namespace CreditApplication.Application.Abstractions.Persistence;

public interface ICreditApplicationRepository
{
    Task<CreditApplicationEntity?> GetByIdAsync(
        Guid id,
        Guid userId,
        CancellationToken cancellationToken);

    Task<PagedResult<CreditApplicationEntity>> ListAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    void Add(CreditApplicationEntity creditApplication);

    void Remove(CreditApplicationEntity creditApplication);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
