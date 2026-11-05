
using CreditApplication.Application.Common;

namespace CreditApplication.Application.CreditApplications;

public interface ICreditApplicationService
{
    Task<CreditApplicationDto> CreateAsync(
        Guid userId,
        CreateCreditApplicationCommand command,
        CancellationToken cancellationToken);

    Task<CreditApplicationDto?> GetByIdAsync(
        Guid id,
        Guid userId,
        CancellationToken cancellationToken);

    Task<PagedResult<CreditApplicationDto>> ListAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    Task<CreditApplicationDto?> UpdateAsync(
        Guid id,
        Guid userId,
        UpdateCreditApplicationCommand command,
        CancellationToken cancellationToken);

    Task<bool> DeleteAsync(
        Guid id,
        Guid userId,
        CancellationToken cancellationToken);
}
