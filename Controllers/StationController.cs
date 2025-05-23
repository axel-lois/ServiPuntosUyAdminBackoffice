using Microsoft.AspNetCore.Mvc;
using ServiPuntosUyAdmin.Models;
using System.Collections.Generic;
using System.Linq;

namespace ServiPuntosUyAdmin.Controllers
{
    public class StationController : Controller
    {
        // Simulación de almacenamiento en memoria
        private static List<Station> stations = FakeData.Stations; // Reutilizamos la lista de estaciones

        private static List<Tenant> tenants = FakeData.Tenants; // Reutilizamos la lista de tenants

        public IActionResult Index()
        {
            ViewData["Title"] = "Estaciones";
            ViewBag.Tenants = tenants;
            return View(stations);
        }

        // GET: Station/Create
        public IActionResult Create()
        {
            ViewBag.Tenants = tenants;
            return View();
        }

        // POST: Station/Create
        [HttpPost]
        public IActionResult Create(Station station)
        {
            if (ModelState.IsValid)
            {
                station.Id = stations.Any() ? stations.Max(s => s.Id) + 1 : 1;
                stations.Add(station);
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Tenants = tenants;
            return View(station);
        }

        // GET: Station/Edit/5
        public IActionResult Edit(int id)
        {
            var station = stations.FirstOrDefault(s => s.Id == id);
            if (station == null) return NotFound();
            ViewBag.Tenants = tenants;
            return View(station);
        }

        // POST: Station/Edit/5
        [HttpPost]
        public IActionResult Edit(Station station)
        {
            var original = stations.FirstOrDefault(s => s.Id == station.Id);
            if (original == null) return NotFound();

            if (ModelState.IsValid)
            {
                original.Nombre = station.Nombre;
                original.Direccion = station.Direccion;
                original.TenantId = station.TenantId;
                original.Activo = station.Activo;
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Tenants = tenants;
            return View(station);
        }

        // GET: Station/Delete/5
        public IActionResult Delete(int id)
        {
            var station = stations.FirstOrDefault(s => s.Id == id);
            if (station == null) return NotFound();
            return View(station);
        }

        // POST: Station/Delete/5
        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            var station = stations.FirstOrDefault(s => s.Id == id);
            if (station != null) stations.Remove(station);
            return RedirectToAction(nameof(Index));
        }
    }
}
