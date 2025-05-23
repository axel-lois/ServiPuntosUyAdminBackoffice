using System.ComponentModel.DataAnnotations;

namespace ServiPuntosUyAdmin.Models
{
    public class Tenant
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres")]
        public string Nombre { get; set; }

        [Display(Name = "Logo URL")]
        [Url(ErrorMessage = "Debe ser una URL válida")]
        public string LogoUrl { get; set; }

        [Display(Name = "Esquema de Color")]
        [Required(ErrorMessage = "El esquema de color es obligatorio")]
        public string EsquemaColor { get; set; }

        [Display(Name = "Activo")]
        public bool Activo { get; set; }
    }
}
