using MediatR;
using System.Text.Json.Serialization;

namespace delivery_rental_system.Application.Queries;

public sealed record GetRentalByIdQuery(string Id)
    : IRequest<RentalDto?>;

public sealed record RentalDto(
    [property: JsonPropertyName("identificador")]
    string Identificador,

    [property: JsonPropertyName("valor_diaria")]
    decimal ValorDiaria,

    [property: JsonPropertyName("entregador_id")]
    string EntregadorId,

    [property: JsonPropertyName("moto_id")]
    string MotoId,

    [property: JsonPropertyName("data_inicio")]
    DateTime DataInicio,

    [property: JsonPropertyName("data_termino")]
    DateTime DataTermino,

    [property: JsonPropertyName("data_previsao_termino")]
    DateTime DataPrevisaoTermino,

    [property: JsonPropertyName("data_devolucao")]
    DateTime? DataDevolucao
);