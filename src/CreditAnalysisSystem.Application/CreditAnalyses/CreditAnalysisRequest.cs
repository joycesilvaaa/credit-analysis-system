using System.ComponentModel.DataAnnotations;
using CreditAnalysisSystem.Domain.Entities;

namespace CreditAnalysisSystem.Application.CreditAnalyses;

/// <summary>Dados para criar ou atualizar uma análise.</summary>
public sealed record CreditAnalysisRequest
{
    [Required(ErrorMessage = "O nome é obrigatório.")]
    [StringLength(CreditAnalysis.NameMaxLength, ErrorMessage = "O nome deve ter no máximo {1} caracteres.")]
    public string Name { get; init; } = string.Empty;

    [Range(typeof(decimal), "0.01", "79228162514264337593543950335", ErrorMessage = "A renda deve ser maior que zero.")]
    public decimal Income { get; init; }

    [Range(CreditAnalysis.MinScore, CreditAnalysis.MaxScore, ErrorMessage = "O score deve estar entre {1} e {2}.")]
    public int Score { get; init; }
}
