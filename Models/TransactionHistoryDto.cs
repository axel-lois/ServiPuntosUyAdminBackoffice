using System;
using Newtonsoft.Json;

namespace ServiPuntosUyAdmin.Models
{
    public class TransactionHistoryDto
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("branchId")]
        public int BranchId { get; set; }
        [JsonProperty("userId")]
        public int UserId { get; set; }
        [JsonProperty("createdAt")]
        public DateTime CreatedAt { get; set; }
        [JsonProperty("amount")]
        public decimal Amount { get; set; }
        [JsonProperty("pointsEarned")]
        public int PointsEarned { get; set; }
        [JsonProperty("type")]
        public int Type { get; set; }
        [JsonProperty("pointsSpent")]
        public int PointsSpent { get; set; }
    }
}
