using System.Text.Json.Serialization;

namespace Accessory_DesktopApp.Models;

public sealed class StatisticsDto
{
    [JsonPropertyName("count_customers")]
    public int CountCustomers { get; set; }

    [JsonPropertyName("count_products")]
    public int CountProducts { get; set; }

    [JsonPropertyName("count_orders")]
    public int CountOrders { get; set; }
}
