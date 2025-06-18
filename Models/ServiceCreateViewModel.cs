using System.ComponentModel.DataAnnotations;

namespace ServiPuntosUyAdmin.Models
{
    public class ServiceCreateViewModel
    {
        [Required]
        public int BranchId { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; }

        public string Description { get; set; }

        [Required, Range(0, 100000)]
        public decimal Price { get; set; }

        public bool AgeRestricted { get; set; }

        [Required, DataType(DataType.Time)]
        public string StartTime { get; set; }

        [Required, DataType(DataType.Time)]
        public string EndTime { get; set; }
    }
}
