using Microsoft.AspNetCore.Mvc;
using ServiPuntosUyAdmin.Models;
using System.Collections.Generic;
using System.Linq;

namespace ServiPuntosUyAdmin.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TenantApiController : ControllerBase
    {
        // Reutilizamos la lista en memoria
        private static List<Tenant> tenants = FakeData.Tenants;

        // POST: api/TenantApi/Create
        [HttpPost("Create")]
        public IActionResult Create([FromBody] Tenant tenant)
        {
            if (tenant == null || string.IsNullOrWhiteSpace(tenant.Nombre))
                return BadRequest(new { message = "El nombre es obligatorio" });

            tenant.Id = tenants.Any() ? tenants.Max(t => t.Id) + 1 : 1;
            tenants.Add(tenant);

            return Ok(tenant); // Devuelve el tenant creado
        }

        // GET: api/TenantApi/List
        [HttpGet("List")]
        public IActionResult List()
        {
            return Ok(tenants);
        }

        // TODO. agregar editar y eliminar
    }
}
