using System.ComponentModel.DataAnnotations;

namespace ServiPuntosUyAdmin.Models
{
    public class LoyaltyProgramViewModel
    {
        [Required]
        public string PointsName { get; set; }

        [Required, Range(1, int.MaxValue)]
        public int PointsValue { get; set; }

        [Required]
        public decimal AccumulationRule { get; set; }

        [Required]
        public int ExpiricyPolicyDays { get; set; }
    }
}
