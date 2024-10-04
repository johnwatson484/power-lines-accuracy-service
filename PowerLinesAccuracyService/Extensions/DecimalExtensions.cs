namespace PowerLinesAccuracyService.Extensions;

public static class DecimalExtensions
{
    public static decimal SafeDivide(this decimal numerator, decimal denominator)
    {
        return denominator == 0 ? 0 : decimal.Divide(numerator, denominator);
    }
}
