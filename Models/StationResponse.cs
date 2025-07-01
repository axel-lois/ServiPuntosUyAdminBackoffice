using System.Text.Json.Serialization;

namespace ServiPuntosUyAdmin.Models
{
    public class StationResponse
    {
        [JsonPropertyName("error")]
        public bool Error { get; set; }

        [JsonPropertyName("data")]
        public Station Data { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }
    }
}
