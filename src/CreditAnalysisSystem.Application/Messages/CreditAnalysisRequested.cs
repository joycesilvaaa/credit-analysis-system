namespace CreditAnalysisSystem.Application.Messages;

/// <summary>
/// Mensagem publicada na fila quando uma análise precisa ser avaliada.
/// Carrega apenas o Id: o worker sempre lê os dados atualizados do banco.
/// </summary>
public sealed record CreditAnalysisRequested(int AnalysisId);
