namespace CreditAnalysisSystem.Domain.Policies;

/// <summary>Regras usadas para aprovar ou reprovar uma análise de crédito.</summary>
public static class CreditPolicy
{
    public const int MinimumScore = 700;
    public const decimal MinimumIncome = 2000m;

    private static readonly System.Globalization.CultureInfo BrazilianCulture =
        System.Globalization.CultureInfo.GetCultureInfo("pt-BR");

    public static CreditDecision Evaluate(int score, decimal income)
    {
        if (score < MinimumScore)
            return CreditDecision.Reject($"Score abaixo do mínimo exigido ({MinimumScore}).");

        if (income < MinimumIncome)
            return CreditDecision.Reject(
                $"Renda abaixo do mínimo exigido ({MinimumIncome.ToString("C", BrazilianCulture)}).");

        return CreditDecision.Approve();
    }
}

public sealed record CreditDecision(bool Approved, string? RejectionReason)
{
    public static CreditDecision Approve() => new(true, null);
    public static CreditDecision Reject(string reason) => new(false, reason);
}
