using System.Text.Json.Serialization;

namespace ServiPuntosUyAdmin.Models
{
    public class ServiceResponse
    {
        [JsonPropertyName("error")]
        public bool Error { get; set; }

        // Aquí cae el objeto real del servicio
        [JsonPropertyName("data")]
        public Service Data { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }
    }
}
