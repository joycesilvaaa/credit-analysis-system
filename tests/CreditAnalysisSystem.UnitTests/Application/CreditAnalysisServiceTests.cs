using CreditAnalysisSystem.Application.CreditAnalyses;
using CreditAnalysisSystem.Application.Exceptions;
using CreditAnalysisSystem.Domain.Entities;
using CreditAnalysisSystem.Domain.Enums;
using CreditAnalysisSystem.UnitTests.Fakes;
using Microsoft.Extensions.Logging.Abstractions;

namespace CreditAnalysisSystem.UnitTests.Application;

public class CreditAnalysisServiceTests
{
    private readonly FakeCreditAnalysisRepository _repository = new();
    private readonly FakeCreditAnalysisPublisher _publisher = new();
    private readonly CreditAnalysisService _service;

    public CreditAnalysisServiceTests() =>
        _service = new CreditAnalysisService(_repository, _publisher, NullLogger<CreditAnalysisService>.Instance);

    private static CreditAnalysisRequest Request(int score = 750) =>
        new() { Name = "Maria", Income = 5000, Score = score };

    [Fact]
    public async Task Create_saves_pending_and_publishes_message()
    {
        var result = await _service.CreateAsync(Request());

        Assert.Equal(CreditAnalysisStatus.Pending, result.Status);
        Assert.Single(_repository.Items);
        Assert.Equal(result.Id, Assert.Single(_publisher.Published).AnalysisId);
    }

    [Fact]
    public async Task Create_still_succeeds_when_publish_fails()
    {
        _publisher.Fail = true;

        var result = await _service.CreateAsync(Request());

        Assert.Equal(CreditAnalysisStatus.Pending, result.Status);
        Assert.Single(_repository.Items);
    }

    [Fact]
    public async Task GetById_throws_not_found_for_unknown_id()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _service.GetByIdAsync(99));
    }

    [Fact]
    public async Task Update_resets_to_pending_and_republishes()
    {
        var analysis = _repository.Seed(new CreditAnalysis("João", 5000, 500));
        analysis.Evaluate();

        var result = await _service.UpdateAsync(analysis.Id, Request(score: 720));

        Assert.Equal(CreditAnalysisStatus.Pending, result.Status);
        Assert.Equal(720, result.Score);
        Assert.Equal(analysis.Id, Assert.Single(_publisher.Published).AnalysisId);
    }

    [Fact]
    public async Task Delete_pending_analysis_throws_conflict()
    {
        var analysis = _repository.Seed(new CreditAnalysis("Maria", 5000, 750));

        await Assert.ThrowsAsync<ConflictException>(() => _service.DeleteAsync(analysis.Id));
        Assert.Single(_repository.Items);
    }

    [Fact]
    public async Task Delete_evaluated_analysis_removes_it()
    {
        var analysis = _repository.Seed(new CreditAnalysis("Maria", 5000, 750));
        analysis.Evaluate();

        await _service.DeleteAsync(analysis.Id);

        Assert.Empty(_repository.Items);
    }

    [Fact]
    public async Task List_filters_by_status()
    {
        _repository.Seed(new CreditAnalysis("Pendente", 5000, 750));
        _repository.Seed(new CreditAnalysis("Aprovada", 5000, 750)).Evaluate();

        var approved = await _service.ListAsync(CreditAnalysisStatus.Approved);

        Assert.Equal("Aprovada", Assert.Single(approved).Name);
    }
}
