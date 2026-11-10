using CreditAnalysisSystem.Domain.Entities;
using CreditAnalysisSystem.Domain.Enums;
using CreditAnalysisSystem.Domain.Exceptions;

namespace CreditAnalysisSystem.UnitTests.Domain;

public class CreditAnalysisTests
{
    [Fact]
    public void New_analysis_starts_pending()
    {
        var analysis = new CreditAnalysis("  Maria  ", 5000, 750);

        Assert.Equal(CreditAnalysisStatus.Pending, analysis.Status);
        Assert.Equal("Maria", analysis.Name);
        Assert.Null(analysis.ProcessedAt);
        Assert.False(analysis.CanBeDeleted);
    }

    [Theory]
    [InlineData("", 5000, 750)]
    [InlineData("   ", 5000, 750)]
    [InlineData("Maria", 0, 750)]
    [InlineData("Maria", -1, 750)]
    [InlineData("Maria", 5000, -1)]
    [InlineData("Maria", 5000, 1001)]
    public void Invalid_data_is_rejected(string name, decimal income, int score)
    {
        Assert.Throws<DomainException>(() => new CreditAnalysis(name, income, score));
    }

    [Fact]
    public void Name_longer_than_limit_is_rejected()
    {
        var name = new string('a', CreditAnalysis.NameMaxLength + 1);
        Assert.Throws<DomainException>(() => new CreditAnalysis(name, 5000, 750));
    }

    [Theory]
    [InlineData(700, 2000)]
    [InlineData(1000, 100000)]
    public void Evaluate_approves_when_score_and_income_reach_minimum(int score, decimal income)
    {
        var analysis = new CreditAnalysis("Maria", income, score);

        analysis.Evaluate();

        Assert.Equal(CreditAnalysisStatus.Approved, analysis.Status);
        Assert.Null(analysis.RejectionReason);
        Assert.NotNull(analysis.ProcessedAt);
        Assert.True(analysis.CanBeDeleted);
    }

    [Theory]
    [InlineData(699, 5000, "Score")]
    [InlineData(800, 1999.99, "Renda")]
    public void Evaluate_rejects_with_reason(int score, decimal income, string reasonStartsWith)
    {
        var analysis = new CreditAnalysis("João", income, score);

        analysis.Evaluate();

        Assert.Equal(CreditAnalysisStatus.Rejected, analysis.Status);
        Assert.StartsWith(reasonStartsWith, analysis.RejectionReason);
    }

    [Fact]
    public void Evaluate_twice_is_not_allowed()
    {
        var analysis = new CreditAnalysis("Maria", 5000, 750);
        analysis.Evaluate();

        Assert.Throws<DomainException>(analysis.Evaluate);
    }

    [Fact]
    public void Update_resets_to_pending_and_clears_result()
    {
        var analysis = new CreditAnalysis("João", 5000, 500);
        analysis.Evaluate();

        analysis.Update("João Silva", 6000, 720);

        Assert.Equal(CreditAnalysisStatus.Pending, analysis.Status);
        Assert.Null(analysis.RejectionReason);
        Assert.Null(analysis.ProcessedAt);
        Assert.Equal(720, analysis.Score);
    }
}
