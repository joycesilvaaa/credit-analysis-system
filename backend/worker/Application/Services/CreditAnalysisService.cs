using backend.worker.DTOs;
using backend.worker.Repositories;

namespace backend.worker.Services;

public class CreditAnalysisService(ICreditAnalysisRepository repository)
    : ICreditAnalysisService
{

    public async Task ProcessAsync(
        CreditAnalysisMessage message)
    {
        var analysis = await repository
            .GetByIdAsync(message.Id);

        if (analysis is null)
            return;

        await Task.Delay(3000);

        analysis.Result =
            message.Score >= 700 ? 1 : 0;
        analysis.Status = "Finalizado";
        analysis.UpdatedAt = DateTime.UtcNow;
        await repository.SaveChangesAsync();
    }
}