using System.Collections.Generic;

namespace ServiPuntosUyAdmin.Models
{
    public static class FakeData
    {
        public static List<Tenant> Tenants = new List<Tenant>
        {
            new Tenant { Id = 1, Nombre = "Ancap", LogoUrl = "", EsquemaColor = "#ffc107", Activo = true },
            new Tenant { Id = 2, Nombre = "Petrobras", LogoUrl = "", EsquemaColor = "#28a745", Activo = true }
        };

        public static List<Station> Stations = new List<Station>
        {
            new Station
            {
                Id = 1,
                TenantId = 1,
                Address = "Av. 18 de Julio 1234",
                Latitud = "-34.9011",
                Longitud = "-56.1645",
                Phone = "24001234",
                OpenTime = "08:00",
                ClosingTime = "20:00"
            },
            new Station
            {
                Id = 2,
                TenantId = 2,
                Address = "Bulevar Artigas 5678",
                Latitud = "-34.9100",
                Longitud = "-56.1800",
                Phone = "26007890",
                OpenTime = "09:00",
                ClosingTime = "19:00"
            }
        };
    }
}
