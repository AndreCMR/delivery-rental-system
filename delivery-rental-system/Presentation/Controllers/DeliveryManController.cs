using delivery_rental_system.Application.Commands.Create.DeliveryMan;
using delivery_rental_system.Application.Commands.Update;
using delivery_rental_system.Presentation.Requests.DeliveryMan;
using delivery_rental_system.Presentation.Responses;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace delivery_rental_system.Presentation.Controllers;

[ApiController]
[Route("entregadores")]
[Tags("entregadores")]
public sealed class DeliveryManController(ISender _sender) : ControllerBase
{
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiMessageResponse), StatusCodes.Status400BadRequest)]
    [SwaggerOperation(Summary = "Cadastrar Entregador")]
    public async Task<IActionResult> Create([FromBody] CreateDeliveryManRequest body, CancellationToken ct)
    {
        try
        {
            await _sender.Send(new CreateDeliveryManCommand(
                body.Identificador,
                body.Nome,
                body.Cnpj,
                body.DataNascimento,
                body.NumeroCnh,
                body.TipoCnh,
                body.ImagemCnh
            ), ct);
                
            return StatusCode(StatusCodes.Status201Created);
        }
        catch
        {
            return BadRequest(new ApiMessageResponse("Dados inválidos"));
        }
    }

    [HttpPost("{id}/cnh")]
    [SwaggerOperation(Summary = "Enviar foto da CNH")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiMessageResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadCnhJson(
      [FromRoute] string id,
      [FromBody] UploadCnhRequest body,
      [FromServices] ISender sender,
      CancellationToken ct)
    {
        await sender.Send(new UploadDeliveryManCnhCommand(id, body.ImagemCnh), ct);

        return Created($"/entregadores/{id}", null);
    }
}