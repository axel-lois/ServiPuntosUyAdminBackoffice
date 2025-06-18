using System;
using System.ComponentModel.DataAnnotations;

namespace ServiPuntosUyAdmin.Models
{
    public class PromotionCreateViewModel
    {
        // Inyectados desde sesión
        public int BranchId { get; set; }
        public int TenantId { get; set; }

        [Required]
        [Display(Name = "Descripción")]
        public string Description { get; set; }

        [Required]
        [Display(Name = "Fecha inicio")]
        [DataType(DataType.DateTime)]
        public DateTime StartDate { get; set; }

        [Required]
        [Display(Name = "Fecha fin")]
        [DataType(DataType.DateTime)]
        public DateTime EndDate { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        [Display(Name = "Precio")]
        public decimal Price { get; set; }
    }
}
