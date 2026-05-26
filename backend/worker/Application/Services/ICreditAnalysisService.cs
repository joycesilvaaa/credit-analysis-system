using backend.worker.DTOs;


namespace backend.worker.Services;

public interface ICreditAnalysisService
{
    Task ProcessAsync(CreditAnalysisMessage message);
}