using CreditAnalysisSystem.Domain.Enums;
using CreditAnalysisSystem.Domain.Exceptions;
using CreditAnalysisSystem.Domain.Policies;

namespace CreditAnalysisSystem.Domain.Entities;

public class CreditAnalysis
{
    public const int NameMaxLength = 150;
    public const int MinScore = 0;
    public const int MaxScore = 1000;

    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public decimal Income { get; private set; }
    public int Score { get; private set; }
    public CreditAnalysisStatus Status { get; private set; }
    public string? RejectionReason { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public DateTime? ProcessedAt { get; private set; }

    // Usado pelo EF Core
    private CreditAnalysis() { }

    public CreditAnalysis(string name, decimal income, int score)
    {
        SetData(name, income, score);
        Status = CreditAnalysisStatus.Pending;
        CreatedAt = UpdatedAt = DateTime.UtcNow;
    }

    public bool CanBeDeleted => Status != CreditAnalysisStatus.Pending;

    /// <summary>Altera os dados e devolve a análise para a fila de avaliação.</summary>
    public void Update(string name, decimal income, int score)
    {
        SetData(name, income, score);
        Status = CreditAnalysisStatus.Pending;
        RejectionReason = null;
        ProcessedAt = null;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>Aplica a política de crédito e define o resultado.</summary>
    public void Evaluate()
    {
        if (Status != CreditAnalysisStatus.Pending)
            throw new DomainException($"A análise {Id} já foi avaliada.");

        var decision = CreditPolicy.Evaluate(Score, Income);

        Status = decision.Approved ? CreditAnalysisStatus.Approved : CreditAnalysisStatus.Rejected;
        RejectionReason = decision.RejectionReason;
        ProcessedAt = UpdatedAt = DateTime.UtcNow;
    }

    private void SetData(string name, decimal income, int score)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("O nome é obrigatório.");
        if (name.Trim().Length > NameMaxLength)
            throw new DomainException($"O nome deve ter no máximo {NameMaxLength} caracteres.");
        if (income <= 0)
            throw new DomainException("A renda deve ser maior que zero.");
        if (score is < MinScore or > MaxScore)
            throw new DomainException($"O score deve estar entre {MinScore} e {MaxScore}.");

        Name = name.Trim();
        Income = income;
        Score = score;
    }
}
