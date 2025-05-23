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
            new Station { Id = 1, Nombre = "Estación Centro", Direccion = "Av. 18 de Julio 1234", TenantId = 1, Activo = true },
            new Station { Id = 2, Nombre = "Estación Pocitos", Direccion = "Bulevar España 2233", TenantId = 2, Activo = true }
        };
    }
}
