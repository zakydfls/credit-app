using CreditApplication.Application.CreditApplications;

namespace CreditApplication.Api.Contracts.CreditApplications;

public sealed record CreditApplicationResponse(
    Guid Id,
    decimal Plafon,
    decimal Bunga,
    int Tenor,
    decimal Angsuran,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    Guid UserId)
{
    public static CreditApplicationResponse FromDto(CreditApplicationDto dto) =>
        new(dto.Id, dto.Plafon, dto.Bunga, dto.Tenor, dto.Angsuran,
            dto.CreatedAt, dto.UpdatedAt, dto.UserId);
}
