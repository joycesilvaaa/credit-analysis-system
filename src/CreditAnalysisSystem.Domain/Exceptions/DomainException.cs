namespace CreditAnalysisSystem.Domain.Exceptions;

/// <summary>Violação de uma regra de negócio.</summary>
public class DomainException(string message) : Exception(message);
