using System.Text.Json.Serialization;

namespace Accessory_DesktopApp.Models;

public sealed class MonthlyRevenueDto
{
    [JsonPropertyName("label")]
    public string Label { get; set; } = string.Empty;

    [JsonPropertyName("total")]
    public decimal Total { get; set; }
}
