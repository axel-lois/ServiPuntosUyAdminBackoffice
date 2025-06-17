// Models/StationHoursDto.cs
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ServiPuntosUyAdmin.Models
{
    public class StationHoursDto
    {
        [JsonPropertyName("branchId")]
        public int BranchId { get; set; }

        [Required]
        [Display(Name = "Horario Apertura")]
        [JsonPropertyName("openTime")]
        public string OpenTime { get; set; }

        [Required]
        [Display(Name = "Horario Cierre")]
        [JsonPropertyName("closingTime")]
        public string ClosingTime { get; set; }
    }
}
