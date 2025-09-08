using delivery_rental_system.Application.Commands;
using delivery_rental_system.Application.Commands.Create;
using delivery_rental_system.Application.Commands.Delete;
using delivery_rental_system.Application.Queries.Motorcycles;
using delivery_rental_system.Presentation.Requests;
using delivery_rental_system.Presentation.Responses;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace delivery_rental_system.Presentation.Controllers;

[ApiController]
[Route("motos")]
[Tags("motos")]
public sealed class MotorcyclesController(ISender _sender) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [SwaggerOperation(Summary = "Cadastrar uma nova moto")]
    public async Task<IActionResult> Create([FromBody] CreateMotorcycleRequest request, CancellationToken ct)
    {
        try
        {
            await _sender.Send(new CreateMotorcycleCommand(request.Identificador, request.Ano, request.Modelo, request.Placa), ct);

            return StatusCode(StatusCodes.Status201Created);
        }
        catch
        {
            return BadRequest(new ApiMessageResponse("Dados inválidos"));
        }
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(MotorcycleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Consultar motos existentes por id")]
    public async Task<IActionResult> GetById(string id, CancellationToken ct)
    {
        var dto = await _sender.Send(new GetMotorcycleByIdQuery(id), ct);
        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpGet]
    [Produces("application/json")]
    [ProducesResponseType(typeof(IReadOnlyList<MotorcycleDto>), StatusCodes.Status200OK)]
    [SwaggerOperation(Summary = "Consultar motos existentes")]
    public async Task<IActionResult> Search([FromQuery(Name = "placa")] string? placa, CancellationToken ct)
    {
        var list = await _sender.Send(new SearchMotorcyclesByPlateQuery(placa), ct);
        return Ok(list);
    }


    [HttpPut("{id}/placa")]
    [ProducesResponseType(typeof(UpdateMotorcyclePlateResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [SwaggerOperation(Summary = "Modificar a placa de uma moto")]

    public async Task<IActionResult> UpdatePlate([FromRoute] string id, [FromBody] UpdateMotorcyclePlateRequest request)
    {
        try
        {
            await _sender.Send(new UpdateMotorcyclePlateCommand(id, request.Placa));

            return Ok(new ApiMessageResponse("Placa modificada com sucesso"));
        }
        catch
        {
            return BadRequest(new ApiMessageResponse("Dados inválidos"));
        }
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [SwaggerOperation(Summary = "Remover uma moto")]
    public async Task<IActionResult> Delete([FromRoute] string id, ISender sender)
    {
        var result = await sender.Send(new DeleteMotorcycleCommand(id));

        if (result is not null && result.Mensagem == "Dados inválidos")
            return BadRequest(result);

        return Ok(); 
    }

}
