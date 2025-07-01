// Models/PromotionProductDto.cs
using System.Text.Json.Serialization;

namespace ServiPuntosUyAdmin.Models
{
    public class PromotionProductDto
    {
        [JsonPropertyName("productId")]
        public int ProductId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
}
