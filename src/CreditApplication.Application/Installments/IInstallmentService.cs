namespace CreditApplication.Application.Installments;

public interface IInstallmentService
{
    decimal Calculate(decimal plafon, decimal bunga, int tenor);
}
