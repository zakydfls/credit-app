namespace CreditApplication.Application.CreditApplications;

public sealed record CreditApplicationDto(
    Guid Id,
    decimal Plafon,
    decimal Bunga,
    int Tenor,
    decimal Angsuran,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    Guid UserId);
