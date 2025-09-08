using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace delivery_rental_system.Presentation.Requests.DeliveryMan;

public sealed class UploadCnhRequest
{
    [Required]
    [JsonPropertyName("imagem_cnh")]
    public string ImagemCnh { get; set; } = default!;
}