using Microsoft.AspNetCore.Mvc;
using ServiPuntosUyAdmin.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Linq;

namespace ServiPuntosUyAdmin.Controllers
{
    public class StationController : Controller
    {
        private readonly string apiBaseUrl = "http://localhost:5162/api/Branch";

        // GET: Station/Index
        public async Task<IActionResult> Index()
        {
            // Obtener el token de la sesión
            string token = HttpContext.Session.GetString("jwt_token");
            // Para poblar el combo de tenants, lo seguimos haciendo con FakeData, o podés hacerlo desde API
            var tenants = FakeData.Tenants;
            ViewBag.Tenants = tenants;

            // En este ejemplo NO hay un endpoint para listar todas, así que mostramos vacío
            // Cuando lo tengas, hacé el request aquí
            var stations = new List<Station>();
            ViewBag.Info = "No hay endpoint para listar estaciones aún.";
            return View(stations);
        }

        // GET: Station/Create
        public IActionResult Create()
        {
            ViewBag.Tenants = FakeData.Tenants;
            return View();
        }

        // POST: Station/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Station station)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Tenants = FakeData.Tenants;
                return View(station);
            }

            using (var client = new HttpClient())
            {
                string token = HttpContext.Session.GetString("jwt_token");
                if (!string.IsNullOrEmpty(token))
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var json = JsonConvert.SerializeObject(new
                {
                    tenantId = station.TenantId,
                    address = station.Address,
                    latitud = station.Latitud,
                    longitud = station.Longitud,
                    phone = station.Phone,
                    openTime = station.OpenTime,
                    closingTime = station.ClosingTime
                });
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync($"{apiBaseUrl}/Create", content);

                string apiResponse = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "¡La estación se creó correctamente!";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ViewBag.Tenants = FakeData.Tenants;
                    ViewBag.Error = "No se pudo crear la estación. Detalles: " + apiResponse;
                    return View(station);
                }

            }
        }

        // GET: Station/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            ViewBag.Tenants = FakeData.Tenants;
            // Buscar una estación por su ID usando el endpoint /api/Branch/{id}
            using (var client = new HttpClient())
            {
                string token = HttpContext.Session.GetString("jwt_token");
                if (!string.IsNullOrEmpty(token))
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await client.GetAsync($"{apiBaseUrl}/{id}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var station = JsonConvert.DeserializeObject<Station>(json);
                    if (station == null) return NotFound();
                    return View(station);
                }
                else
                {
                    ViewBag.Error = "No se pudo obtener la estación.";
                    return RedirectToAction(nameof(Index));
                }
            }
        }

        // POST: Station/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Station station)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Tenants = FakeData.Tenants;
                return View(station);
            }

            using (var client = new HttpClient())
            {
                string token = HttpContext.Session.GetString("jwt_token");
                if (!string.IsNullOrEmpty(token))
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var json = JsonConvert.SerializeObject(new
                {
                    address = station.Address,
                    latitud = station.Latitud,
                    longitud = station.Longitud,
                    phone = station.Phone,
                    openTime = station.OpenTime,
                    closingTime = station.ClosingTime
                });
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PutAsync($"{apiBaseUrl}/{station.Id}/Update", content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "¡La estación se editó correctamente!";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ViewBag.Tenants = FakeData.Tenants;
                    ViewBag.Error = "No se pudo editar la estación.";
                    return View(station);
                }
            }
        }

        // GET: Station/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            // Cargar datos para confirmar eliminación
            using (var client = new HttpClient())
            {
                string token = HttpContext.Session.GetString("jwt_token");
                if (!string.IsNullOrEmpty(token))
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await client.GetAsync($"{apiBaseUrl}/{id}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var station = JsonConvert.DeserializeObject<Station>(json);
                    if (station == null) return NotFound();
                    return View(station);
                }
                else
                {
                    TempData["Error"] = "No se pudo obtener la estación a eliminar.";
                    return RedirectToAction(nameof(Index));
                }
            }
        }

        // POST: Station/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            using (var client = new HttpClient())
            {
                string token = HttpContext.Session.GetString("jwt_token");
                if (!string.IsNullOrEmpty(token))
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await client.DeleteAsync($"{apiBaseUrl}/{id}/Delete");
                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "¡La estación se eliminó correctamente!";
                }
                else
                {
                    TempData["Error"] = "No se pudo eliminar la estación en el backend";
                }
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
