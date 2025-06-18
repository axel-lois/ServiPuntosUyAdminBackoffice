using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ServiPuntosUyAdmin.Models
{
    // Wrapper para GET /api/Promotion/branch
    public class PromotionListResponse
    {
        [JsonPropertyName("error")]
        public bool Error { get; set; }

        [JsonPropertyName("data")]
        public List<Promotion> Data { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }
    }

    // Wrapper para GET /api/Promotion/{id}/{branchId}
    public class PromotionResponse
    {
        [JsonPropertyName("error")]
        public bool Error { get; set; }

        [JsonPropertyName("data")]
        public Promotion Data { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }
    }

    public class Promotion
    {
        [JsonPropertyName("promotionId")]
        public int PromotionId { get; set; }

        [JsonPropertyName("tenantId")]
        public int TenantId { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("startDate")]
        public DateTime StartDate { get; set; }

        [JsonPropertyName("endDate")]
        public DateTime EndDate { get; set; }

        [JsonPropertyName("price")]
        public decimal Price { get; set; }

        // Si tu API devuelve branches[] o products[], agrégalos aquí
        // [JsonPropertyName("branches")] public List<int> Branches { get; set; }
        // [JsonPropertyName("products")] public List<int> Products { get; set; }
    }
}
