using delivery_rental_system.Domain.Enums;
using System.Text.Json.Serialization;

namespace delivery_rental_system.Presentation.Requests.Rental;

public sealed class CreateRentalRequest
{
    [JsonPropertyName("entregador_id")]
    public string EntregadorId { get; set; } = default!;

    [JsonPropertyName("moto_id")]
    public string MotoId { get; set; } = default!; 

    [JsonPropertyName("data_inicio")]
    public DateTime DataInicio { get; set; }

    [JsonPropertyName("data_termino")]
    public DateTime DataTermino { get; set; }

    [JsonPropertyName("data_previsao_termino")]
    public DateTime DataPrevisaoTermino { get; set; }

    [JsonPropertyName("plano")]
    public int Plano { get; set; }
}


