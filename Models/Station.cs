using System.ComponentModel.DataAnnotations;

namespace ServiPuntosUyAdmin.Models
{
    public class Station
    {
        public int Id { get; set; }
        public int TenantId { get; set; }
        public string Address { get; set; }
        public string Latitud { get; set; }
        public string Longitud { get; set; }
        public string Phone { get; set; }
        public string OpenTime { get; set; }
        public string ClosingTime { get; set; }
    }

    // DTO para respuesta con Tenant embebido (opcional)
    public class StationResponseDto
    {
        public int Id { get; set; }
        public int TenantId { get; set; }
        public TenantDto Tenant { get; set; }
        public string Address { get; set; }
        public string Latitud { get; set; }
        public string Longitud { get; set; }
        public string Phone { get; set; }
        public string OpenTime { get; set; }
        public string ClosingTime { get; set; }
    }

    public class TenantDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
