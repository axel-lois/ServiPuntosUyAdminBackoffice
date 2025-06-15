using Newtonsoft.Json;

namespace ServiPuntosUyAdmin.Models
{
    public class ApiResponse<T>
    {
        [JsonProperty("error")]
        public bool Error { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("data")]
        public T Data { get; set; }
    }
}
