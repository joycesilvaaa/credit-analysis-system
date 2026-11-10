namespace CreditAnalysisSystem.Application.Exceptions;

public class ConflictException(string message, Exception? innerException = null)
    : Exception(message, innerException);

/// <summary>O registro foi alterado por outro processo entre a leitura e a gravação.</summary>
public class ConcurrencyConflictException(string message, Exception? innerException = null)
    : ConflictException(message, innerException);
