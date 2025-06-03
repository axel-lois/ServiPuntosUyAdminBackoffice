using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ServiPuntosUyAdmin.Models
{
    public class Station
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("tenantId")]
        public int TenantId { get; set; }

        [JsonPropertyName("address")]
        public string Address { get; set; }

        [JsonPropertyName("latitud")]
        public string Latitud { get; set; }

        [JsonPropertyName("longitud")]
        public string Longitud { get; set; }

        [JsonPropertyName("phone")]
        public string Phone { get; set; }

        [JsonPropertyName("openTime")]
        public string OpenTime { get; set; }

        [JsonPropertyName("closingTime")]
        public string ClosingTime { get; set; }
    }

    // DTO para respuesta con Tenant embebido (opcional)
    public class StationResponseDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("tenantId")]
        public int TenantId { get; set; }
        [JsonPropertyName("tenant")]
        public TenantDto Tenant { get; set; }
        [JsonPropertyName("address")]
        public string Address { get; set; }
        [JsonPropertyName("latitud")]
        public string Latitud { get; set; }
        [JsonPropertyName("longitud")]
        public string Longitud { get; set; }
        [JsonPropertyName("phone")]
        public string Phone { get; set; }
        [JsonPropertyName("openTime")]
        public string OpenTime { get; set; }
        [JsonPropertyName("closingTime")]
        public string ClosingTime { get; set; }
    }

    public class TenantDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
}
