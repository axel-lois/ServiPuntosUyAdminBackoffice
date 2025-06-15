using Newtonsoft.Json;

namespace ServiPuntosUyAdmin.Models
{
    public class TenantUIConfig
    {
        /// <summary>
        /// Id interno de la configuración
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Id de la cadena (tenant) al que pertenece
        /// </summary>
        public int TenantId { get; set; }

        /// <summary>
        /// URL del logo
        /// </summary>
        [JsonProperty("logoUrl")]
        public string LogoUrl { get; set; } = string.Empty;

        /// <summary>
        /// Color primario en formato hex
        /// </summary>
        [JsonProperty("primaryColor")]
        public string PrimaryColor { get; set; } = "#000000";

        /// <summary>
        /// Color secundario en formato hex
        /// </summary>
        [JsonProperty("secondaryColor")]
        public string SecondaryColor { get; set; } = "#ffffff";
    }
}
