using delivery_rental_system.Application.Commands.Create.Rental;
using delivery_rental_system.Application.Queries;
using delivery_rental_system.Presentation.Requests.Rental;
using delivery_rental_system.Presentation.Responses;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace delivery_rental_system.Presentation.Controllers;

[ApiController]
[Route("locacoes")]
public sealed class RentalController(ISender sender) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [SwaggerOperation(Summary = "Alugar uma moto")]
    public async Task<IActionResult> Create([FromBody] CreateRentalRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new CreateRentalCommand(
            request.EntregadorId,
            request.MotoId,
            request.DataInicio,
            request.DataTermino,
            request.DataPrevisaoTermino,
            request.Plano
        ), ct);

        return StatusCode(StatusCodes.Status201Created);
    }

    [HttpPut("{id}/devolucao")]
    [ProducesResponseType(typeof(ReturnPreviewResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiMessageResponse), StatusCodes.Status400BadRequest)]
    [SwaggerOperation(Summary = "Consultar locação por id")]
    public async Task<IActionResult> PreviewReturn([FromRoute] string id, [FromBody] ReturnPreviewRequest body, ISender sender)
    {
        try
        {
            await sender.Send(new SetReturnDateCommand(id, body.ReturnDateUtc));
            return StatusCode(StatusCodes.Status201Created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ApiMessageResponse(ex.Message));
        }
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(RentalDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Informar data de evolução e calcular valor")]
    public async Task<IActionResult> GetById([FromRoute] string id, CancellationToken ct)
    {
        var result = await sender.Send(new GetRentalByIdQuery(id), ct);
        if (result is null) return NotFound(new { mensagem = "Dados inválidos" });

        return Ok(result);
    }
}
