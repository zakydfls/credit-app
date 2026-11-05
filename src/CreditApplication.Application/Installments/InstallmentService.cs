using CreditApplication.Domain.Services;

namespace CreditApplication.Application.Installments;

public sealed class InstallmentService : IInstallmentService
{
    public decimal Calculate(decimal plafon, decimal bunga, int tenor) =>
        InstallmentCalculator.Calculate(plafon, bunga, tenor);
}
