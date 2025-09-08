using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace delivery_rental_system.Presentation.Requests.Rental;
public sealed class ReturnPreviewRequest
{
    [Required]
    [JsonPropertyName("data_devolucao")]
    public DateTime ReturnDateUtc { get; init; }
}