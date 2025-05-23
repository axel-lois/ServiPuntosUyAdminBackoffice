using Microsoft.AspNetCore.Mvc;
using ServiPuntosUyAdmin.Models;
using System.Collections.Generic;
using System.Linq;

namespace ServiPuntosUyAdmin.Controllers
{
    public class TenantController : Controller
    {
        // Simulación de almacenamiento (en memoria)
        private static List<Tenant> tenants = new List<Tenant>
        {
            new Tenant { Id = 1, Nombre = "Ancap", LogoUrl = "", EsquemaColor = "#ffc107", Activo = true },
            new Tenant { Id = 2, Nombre = "Petrobras", LogoUrl = "", EsquemaColor = "#28a745", Activo = true }
        };

        public IActionResult Index()
        {
            ViewData["Title"] = "Cadenas (Tenants)";
            return View(tenants);
        }

        // GET: Tenant/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Tenant/Create
        [HttpPost]
        public IActionResult Create(Tenant tenant)
        {
            if (ModelState.IsValid)
            {
                tenant.Id = tenants.Any() ? tenants.Max(t => t.Id) + 1 : 1;
                tenants.Add(tenant);
                return RedirectToAction(nameof(Index));
            }
            return View(tenant);
        }

        // GET: Tenant/Edit/5
        public IActionResult Edit(int id)
        {
            var tenant = tenants.FirstOrDefault(t => t.Id == id);
            if (tenant == null) return NotFound();
            return View(tenant);
        }

        // POST: Tenant/Edit/5
        [HttpPost]
        public IActionResult Edit(Tenant tenant)
        {
            var original = tenants.FirstOrDefault(t => t.Id == tenant.Id);
            if (original == null) return NotFound();

            original.Nombre = tenant.Nombre;
            original.LogoUrl = tenant.LogoUrl;
            original.EsquemaColor = tenant.EsquemaColor;
            original.Activo = tenant.Activo;

            return RedirectToAction(nameof(Index));
        }

        // GET: Tenant/Delete/5
        public IActionResult Delete(int id)
        {
            var tenant = tenants.FirstOrDefault(t => t.Id == id);
            if (tenant == null) return NotFound();
            return View(tenant);
        }

        // POST: Tenant/Delete/5
        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            var tenant = tenants.FirstOrDefault(t => t.Id == id);
            if (tenant != null) tenants.Remove(tenant);
            return RedirectToAction(nameof(Index));
        }
    }
}
