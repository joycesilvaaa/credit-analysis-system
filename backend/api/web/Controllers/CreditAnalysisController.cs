namespace backend.api.Controllers;

using backend.api.Service;
using backend.shared.Dtos;
using Microsoft.AspNetCore.Mvc;
using backend.shared.Entities;

[ApiController]
[Route("api/credit-analysis")]
public class CreditAnalysisController(ICreditAnalysisService service) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(CreditAnalysis))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CreditAnalysis>> Create([FromBody] CreditAnalysisDto dto)
    {
        if (dto == null) return BadRequest("Os dados da análise não foram informados.");

        var result = await service.CreateAsync(dto);
        
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<CreditAnalysis>))]
    public async Task<ActionResult<IEnumerable<CreditAnalysis>>> GetAll()
    {
        var result = await service.GetAllAsync();
                return Ok(result);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CreditAnalysis))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CreditAnalysis>> GetById(int id)
    {
        var result = await service.GetByIdAsync(id);
        
        if (result == null)
        {
            return NotFound($"Análise com o ID {id} não foi encontrada.");
        }
        
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await service.Delete(id); 
            return NoContent();
        }
        catch (Exception ex) when (ex.Message.Contains("não encontrada"))
        {
            return NotFound(ex.Message);
        }
    }
}