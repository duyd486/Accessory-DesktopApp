using System.Text.Json.Serialization;

namespace Accessory_DesktopApp.Models.Dtos;

public sealed class RevenueByYearDto
{
    [JsonPropertyName("year")]
    public int? Year { get; set; }

    [JsonPropertyName("label")]
    public string? Label { get; set; }

    [JsonPropertyName("total")]
    public decimal Total { get; set; }

    public string DisplayYear => Year?.ToString() ?? Label ?? string.Empty;
}
