using System.ComponentModel.DataAnnotations;

namespace ServiPuntosUyAdmin.Models
{
    public class FuelPriceViewModel
    {
        [Required]
        public int FuelType { get; set; }

        [Required, Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }
    }
}
