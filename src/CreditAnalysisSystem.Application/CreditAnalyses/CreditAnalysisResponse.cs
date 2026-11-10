using CreditAnalysisSystem.Domain.Entities;
using CreditAnalysisSystem.Domain.Enums;

namespace CreditAnalysisSystem.Application.CreditAnalyses;

public sealed record CreditAnalysisResponse(
    int Id,
    string Name,
    decimal Income,
    int Score,
    CreditAnalysisStatus Status,
    string? RejectionReason,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    DateTime? ProcessedAt)
{
    public static CreditAnalysisResponse From(CreditAnalysis a) => new(
        a.Id, a.Name, a.Income, a.Score, a.Status, a.RejectionReason, a.CreatedAt, a.UpdatedAt, a.ProcessedAt);
}
