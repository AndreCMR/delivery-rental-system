using System.Text.Json.Serialization;

namespace delivery_rental_system.Presentation.Responses;

public sealed class ReturnPreviewResponse
{
    [JsonPropertyName("valor_total")]
    public decimal TotalValue { get; init; }

    [JsonPropertyName("valor_diaria")]
    public decimal DailyRate { get; init; }

    [JsonPropertyName("dias_plano")]
    public int PlanDays { get; init; }

    [JsonPropertyName("dias_utilizados")]
    public int UsedDays { get; init; }

    [JsonPropertyName("dias_adicionais")]
    public int ExtraDays { get; init; }

    [JsonPropertyName("multa")]
    public decimal Penalty { get; init; }

    [JsonPropertyName("valor_adicional")]
    public decimal ExtraValue { get; init; }

    [JsonPropertyName("data_inicio")]
    public DateTime StartDate { get; init; }

    [JsonPropertyName("data_previsao_termino")]
    public DateTime PredictedEndDate { get; init; }

    [JsonPropertyName("data_devolucao")]
    public DateTime ReturnDate { get; init; }
}