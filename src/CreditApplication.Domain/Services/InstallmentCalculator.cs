namespace CreditApplication.Domain.Services;

public static class InstallmentCalculator
{
    public static decimal Calculate(
        decimal plafon,
        decimal bungaTahunanPersen,
        int tenorBulan)
    {
        if (plafon <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(plafon), "Plafon harus lebih besar dari 0.");

        if (bungaTahunanPersen <= 0 || bungaTahunanPersen > 100)
            throw new ArgumentOutOfRangeException(
                nameof(bungaTahunanPersen),
                "Bunga harus lebih besar dari 0 dan maksimal 100 persen.");

        if (tenorBulan <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(tenorBulan), "Tenor harus lebih besar dari 0.");

        var bungaBulanan = bungaTahunanPersen / 1200m;
        if (bungaBulanan == 0m)
            throw new ArgumentOutOfRangeException(nameof(bungaTahunanPersen), "Bunga terlalu kecil untuk dihitung.");

        // A discounted factor stays within 0..1 even for very long tenors.
        var faktor = 1m / (1m + bungaBulanan);
        var faktorTenor = 1m;
        var sisaTenor = tenorBulan;
        while (sisaTenor > 0)
        {
            if ((sisaTenor & 1) != 0)
                faktorTenor *= faktor;
            sisaTenor /= 2;
            if (sisaTenor > 0)
                faktor *= faktor;
        }
        var penyebut = 1m - faktorTenor;
        if (penyebut == 0m)
            throw new ArgumentOutOfRangeException(nameof(bungaTahunanPersen), "Bunga terlalu kecil untuk dihitung.");
        var angsuran = plafon * bungaBulanan / penyebut;

        return decimal.Round(
            angsuran,
            2,
            MidpointRounding.AwayFromZero);
    }
}
