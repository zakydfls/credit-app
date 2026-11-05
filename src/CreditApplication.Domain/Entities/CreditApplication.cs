using CreditApplication.Domain.Services;

namespace CreditApplication.Domain.Entities;

public sealed class CreditApplication
{
    public Guid Id { get; private set; }
    public Guid? UserId { get; private set; }
    public decimal Plafon { get; private set; }
    public decimal Bunga { get; private set; }
    public int Tenor { get; private set; }
    public decimal Angsuran { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    private CreditApplication()
    {

    }

    public CreditApplication(Guid userId, decimal plafon, decimal bunga, int tenor)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID tidak valid.", nameof(userId));

        Id = Guid.NewGuid();
        UserId = userId;
        CreatedAt = DateTimeOffset.UtcNow;

        UpdateTerms(plafon, bunga, tenor);
    }

    public void UpdateTerms(decimal plafon, decimal bunga, int tenor)
    {
        var angsuran = InstallmentCalculator.Calculate(plafon, bunga, tenor);

        Plafon = plafon;
        Bunga = bunga;
        Tenor = tenor;
        Angsuran = angsuran;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
