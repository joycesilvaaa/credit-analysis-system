using CreditAnalysisSystem.Application.CreditAnalyses;
using CreditAnalysisSystem.Domain.Entities;
using CreditAnalysisSystem.Domain.Enums;
using CreditAnalysisSystem.UnitTests.Fakes;
using Microsoft.Extensions.Logging.Abstractions;

namespace CreditAnalysisSystem.UnitTests.Application;

public class CreditAnalysisProcessorTests
{
    private readonly FakeCreditAnalysisRepository _repository = new();
    private readonly CreditAnalysisProcessor _processor;

    public CreditAnalysisProcessorTests() =>
        _processor = new CreditAnalysisProcessor(_repository, NullLogger<CreditAnalysisProcessor>.Instance);

    [Fact]
    public async Task Pending_analysis_is_evaluated_and_saved()
    {
        var analysis = _repository.Seed(new CreditAnalysis("Maria", 5000, 750));

        await _processor.ProcessAsync(analysis.Id);

        Assert.Equal(CreditAnalysisStatus.Approved, analysis.Status);
        Assert.Equal(1, _repository.SaveCount);
    }

    [Fact]
    public async Task Already_evaluated_analysis_is_ignored()
    {
        var analysis = _repository.Seed(new CreditAnalysis("Maria", 5000, 750));
        analysis.Evaluate();

        await _processor.ProcessAsync(analysis.Id);

        Assert.Equal(0, _repository.SaveCount);
    }

    [Fact]
    public async Task Missing_analysis_is_ignored()
    {
        await _processor.ProcessAsync(42);

        Assert.Equal(0, _repository.SaveCount);
    }

    [Fact]
    public async Task Concurrency_conflict_is_swallowed()
    {
        var analysis = _repository.Seed(new CreditAnalysis("Maria", 5000, 750));
        _repository.ThrowConcurrencyOnSave = true;

        var exception = await Record.ExceptionAsync(() => _processor.ProcessAsync(analysis.Id));

        Assert.Null(exception);
    }
}
