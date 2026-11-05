using System.ComponentModel.DataAnnotations;
using CreditApplication.Domain.Services;

namespace CreditApplication.Api.Contracts.CreditApplications;

public sealed record CreditApplicationRequest(
    decimal Plafon,
    decimal Bunga,
    int Tenor) : IValidatableObject
{
    private const decimal MaxPlafon = 9_999_999_999_999_999.99m;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Plafon <= 0 || Plafon > MaxPlafon)
        {
            yield return new ValidationResult(
                "Plafon harus lebih besar dari 0 dan maksimal 16 digit sebelum desimal.",
                [nameof(Plafon)]);
        }
        else if (Plafon != decimal.Round(Plafon, 2))
        {
            yield return new ValidationResult(
                "Plafon maksimal memiliki 2 angka desimal.",
                [nameof(Plafon)]);
        }

        if (Bunga <= 0 || Bunga > 100)
        {
            yield return new ValidationResult(
                "Bunga harus lebih besar dari 0 dan maksimal 100 persen.",
                [nameof(Bunga)]);
        }
        else if (Bunga != decimal.Round(Bunga, 2))
        {
            yield return new ValidationResult(
                "Bunga maksimal memiliki 2 angka desimal.",
                [nameof(Bunga)]);
        }

        if (Tenor <= 0)
        {
            yield return new ValidationResult(
                "Tenor harus lebih besar dari 0.",
                [nameof(Tenor)]);
        }

        if (Plafon > 0 && Plafon <= MaxPlafon && Plafon == decimal.Round(Plafon, 2) &&
            Bunga > 0 && Bunga <= 100 && Bunga == decimal.Round(Bunga, 2) && Tenor > 0)
        {
            var angsuran = InstallmentCalculator.Calculate(Plafon, Bunga, Tenor);
            if (angsuran <= 0 || angsuran > MaxPlafon)
                yield return new ValidationResult(
                    "Hasil angsuran harus lebih besar dari 0 dan maksimal 16 digit sebelum desimal.",
                    [nameof(Plafon), nameof(Bunga), nameof(Tenor)]);
        }
    }
}
