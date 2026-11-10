using CreditAnalysisSystem.Application.CreditAnalyses;
using CreditAnalysisSystem.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace CreditAnalysisSystem.Api.Controllers;

[ApiController]
[Route("api/credit-analyses")]
[Produces("application/json")]
public class CreditAnalysesController(ICreditAnalysisService service) : ControllerBase
{
    /// <summary>Cria uma análise e envia para avaliação.</summary>
    [HttpPost]
    [ProducesResponseType<CreditAnalysisResponse>(StatusCodes.Status202Accepted)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(CreditAnalysisRequest request, CancellationToken cancellationToken)
    {
        var result = await service.CreateAsync(request, cancellationToken);
        // 202: a análise foi aceita, mas o resultado sai de forma assíncrona
        return AcceptedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Lista as análises, opcionalmente filtrando por status.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<CreditAnalysisResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] CreditAnalysisStatus? status, CancellationToken cancellationToken) =>
        Ok(await service.ListAsync(status, cancellationToken));

    /// <summary>Consulta uma análise e o seu resultado.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType<CreditAnalysisResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken) =>
        Ok(await service.GetByIdAsync(id, cancellationToken));

    /// <summary>Atualiza os dados e reenvia a análise para avaliação.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType<CreditAnalysisResponse>(StatusCodes.Status202Accepted)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(int id, CreditAnalysisRequest request, CancellationToken cancellationToken)
    {
        var result = await service.UpdateAsync(id, request, cancellationToken);
        return AcceptedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Remove uma análise já avaliada.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await service.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
