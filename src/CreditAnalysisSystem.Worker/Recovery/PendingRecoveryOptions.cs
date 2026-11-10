namespace CreditAnalysisSystem.Worker.Recovery;

public sealed class PendingRecoveryOptions
{
    public const string SectionName = "PendingRecovery";

    /// <summary>De quanto em quanto tempo a rotina roda.</summary>
    public int IntervalSeconds { get; init; } = 60;

    /// <summary>Tempo em Pending a partir do qual a análise é considerada presa.</summary>
    public int StaleAfterSeconds { get; init; } = 120;

    public int BatchSize { get; init; } = 100;
}
