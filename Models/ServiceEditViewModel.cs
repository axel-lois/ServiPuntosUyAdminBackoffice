using System.ComponentModel.DataAnnotations;

namespace ServiPuntosUyAdmin.Models
{
    public class ServiceEditViewModel
    {
        public int Id { get; set; }

        // Lo necesitas para el POST /create o /edit
        public int BranchId { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        public bool AgeRestricted { get; set; }

        [Required]
        [DataType(DataType.Time)]
        public string StartTime { get; set; }

        [Required]
        [DataType(DataType.Time)]
        public string EndTime { get; set; }
    }
}
