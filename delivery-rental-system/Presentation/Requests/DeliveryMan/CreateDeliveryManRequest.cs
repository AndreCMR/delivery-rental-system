using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace delivery_rental_system.Presentation.Requests.DeliveryMan;

public sealed class CreateDeliveryManRequest
{
    [Required]
    [JsonPropertyName("identificador")]
    public string Identificador { get; set; } = null!;

    [Required]
    [JsonPropertyName("nome")]
    public string Nome { get; set; } = null!;

    [Required]
    [JsonPropertyName("cnpj")]
    public string Cnpj { get; set; } = null!; 

    [Required]
    [JsonPropertyName("data_nascimento")]
    public DateTime DataNascimento { get; set; }

    [Required]
    [JsonPropertyName("numero_cnh")]
    public string NumeroCnh { get; set; } = null!;

    [Required]
    [JsonPropertyName("tipo_cnh")]
    public string TipoCnh { get; set; } = null!;

    [JsonPropertyName("imagem_cnh")]
    public string? ImagemCnh { get; set; }
}