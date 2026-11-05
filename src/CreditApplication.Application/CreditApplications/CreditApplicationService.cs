using CreditApplication.Application.Abstractions.Persistence;
using CreditApplication.Application.Common;
using CreditApplicationEntity = CreditApplication.Domain.Entities.CreditApplication;

namespace CreditApplication.Application.CreditApplications;

public sealed class CreditApplicationService(
    ICreditApplicationRepository repository) : ICreditApplicationService
{
    public async Task<CreditApplicationDto> CreateAsync(
        Guid userId,
        CreateCreditApplicationCommand command,
        CancellationToken cancellationToken)
    {
        var entity = new CreditApplicationEntity(
            userId,
            command.Plafon,
            command.Bunga,
            command.Tenor);

        repository.Add(entity);
        await repository.SaveChangesAsync(cancellationToken);

        return ToDto(entity);
    }

    public async Task<CreditApplicationDto?> GetByIdAsync(
        Guid id,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var entity = await repository.GetByIdAsync(id, userId, cancellationToken);
        return entity is null ? null : ToDto(entity);
    }

    public async Task<PagedResult<CreditApplicationDto>> ListAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var result = await repository.ListAsync(userId, page, pageSize, cancellationToken);

        return new PagedResult<CreditApplicationDto>(
            result.Items.Select(ToDto).ToList(),
            result.Page,
            result.PageSize,
            result.TotalCount);
    }

    public async Task<CreditApplicationDto?> UpdateAsync(
        Guid id,
        Guid userId,
        UpdateCreditApplicationCommand command,
        CancellationToken cancellationToken)
    {
        var entity = await repository.GetByIdAsync(id, userId, cancellationToken);

        if (entity is null)
            return null;

        entity.UpdateTerms(command.Plafon, command.Bunga, command.Tenor);
        await repository.SaveChangesAsync(cancellationToken);

        return ToDto(entity);
    }

    public async Task<bool> DeleteAsync(
        Guid id,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var entity = await repository.GetByIdAsync(id, userId, cancellationToken);

        if (entity is null)
            return false;

        repository.Remove(entity);
        await repository.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static CreditApplicationDto ToDto(CreditApplicationEntity entity) =>
        new(
            entity.Id,
            entity.Plafon,
            entity.Bunga,
            entity.Tenor,
            entity.Angsuran,
            entity.CreatedAt,
            entity.UpdatedAt,
            entity.UserId!.Value);
}
