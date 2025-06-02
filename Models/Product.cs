using System.Text.Json.Serialization;

namespace ServiPuntosUyAdmin.Models
{
    public class Product
    {
        [JsonPropertyName("id")]
        public int Id
        {
            get => ProductId;
            set => ProductId = value;
        }

        [JsonPropertyName("productId")]
        public int ProductId { get; set; }

        [JsonPropertyName("tenantId")]
        public int TenantId { get; set; }
        public Tenant Tenant { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public decimal Price { get; set; }
        public bool AgeRestricted { get; set; }
    }
}
