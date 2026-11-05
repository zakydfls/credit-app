namespace CreditApplication.Application.CreditApplications;

public sealed record CreateCreditApplicationCommand(
    decimal Plafon,
    decimal Bunga,
    int Tenor);

public sealed record UpdateCreditApplicationCommand(
    decimal Plafon,
    decimal Bunga,
    int Tenor);