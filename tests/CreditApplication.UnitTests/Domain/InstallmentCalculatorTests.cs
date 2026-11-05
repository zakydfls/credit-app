using CreditApplication.Domain.Services;

namespace CreditApplication.UnitTests.Domain;

public class InstallmentCalculatorTests
{
    [Fact]
    public void Calculate_WithQuestionExample_ReturnsExpectedInstallment()
    {
        var result = InstallmentCalculator.Calculate(
            plafon: 100_000_000m,
            bungaTahunanPersen: 12m,
            tenorBulan: 60);

        Assert.Equal(2_224_444.77m, result);
    }

    [Fact]
    public void Calculate_WithVeryLongTenor_DoesNotOverflow()
    {
        Assert.Equal(1_000_000m, InstallmentCalculator.Calculate(100_000_000m, 12m, int.MaxValue));
    }

    [Fact]
    public void Calculate_WithLargePlafon_RetainsCentPrecision()
    {
        Assert.Equal(10_099_999_999_999_999.99m,
            InstallmentCalculator.Calculate(9_999_999_999_999_999.99m, 12m, 1));
    }

    [Theory]
    [InlineData(0, 12, 60)]
    [InlineData(-1, 12, 60)]
    [InlineData(100_000_000, 0, 60)]
    [InlineData(100_000_000, 100.01, 60)]
    [InlineData(100_000_000, 12, 0)]
    public void Calculate_WithInvalidInput_Throws(
        decimal plafon,
        decimal bungaTahunanPersen,
        int tenorBulan)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            InstallmentCalculator.Calculate(
                plafon,
                bungaTahunanPersen,
                tenorBulan));
    }
}
