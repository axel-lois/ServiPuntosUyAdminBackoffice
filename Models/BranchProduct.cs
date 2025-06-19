using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;   // ← for [Range]
using System.Text.Json.Serialization;
 
namespace ServiPuntosUyAdmin.Models
{
    public class BranchProduct
    {
        [JsonPropertyName("id")]
        public int ProductId { get; set; }

        [JsonPropertyName("name")]
        public string Name     { get; set; }

        [JsonPropertyName("stock")]
        [Range(0, int.MaxValue, ErrorMessage = "El stock debe ser 0 o mayor.")]
        public int Stock       { get; set; }
    }

    public class BranchProductListResponse
    {
        [JsonPropertyName("error")]
        public bool Error                 { get; set; }

        [JsonPropertyName("data")]
        public List<BranchProduct> Data   { get; set; }

        [JsonPropertyName("message")]
        public string Message             { get; set; }
    }

    public class InventoryStockViewModel
    {
        public int ProductId    { get; set; }
        public string ProductName { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Cantidad no válida")]
        public int Quantity     { get; set; }
    }
}
