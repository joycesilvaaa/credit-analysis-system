namespace backend.api.Service;

using shared.Dtos;
using backend.shared.Entities;
using backend.api.Interfaces;
using backend.api.Messaging;
using System.Text.Json;

public class CreditAnalysisService(ICreditAnalysisRepository repository, IRabbitMqPublisher publisher) : ICreditAnalysisService
{
    public async Task<CreditAnalysis> CreateAsync(CreditAnalysisDto dto)
    {
        var analysis = new CreditAnalysis
        {
            Name = dto.name,
            Score = dto.score,
            Status = "Processando"
        };
        await repository.AddAsync(analysis);
        await repository.SaveChangesAsync();
        await publisher.Publish(
            JsonSerializer.Serialize(new
            {
                analysis.Id,
                analysis.Score
            })
        );
        return analysis;
    }

    public async Task<CreditAnalysis?> GetByIdAsync(int id)
    {
        return await repository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<CreditAnalysis>> GetAllAsync()
    {
        return await repository.GetAllAsync();
    }

    public async Task Delete(int id)
    {
        var analysis = await repository.GetByIdAsync(id);
        if (analysis == null)
        {
            throw new Exception("Análise não encontrada");
        }
        await repository.Delete(analysis);
        await repository.SaveChangesAsync();
    }
}
