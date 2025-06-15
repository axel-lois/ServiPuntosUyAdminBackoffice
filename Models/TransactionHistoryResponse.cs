using System.Collections.Generic;
using Newtonsoft.Json;

namespace ServiPuntosUyAdmin.Models
{
    public class TransactionHistoryResponse
    {
        [JsonProperty("error")]
        public bool Error { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("data")]
        public List<TransactionHistoryDto> Data { get; set; }
    }
}
