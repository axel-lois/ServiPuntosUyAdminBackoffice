using Microsoft.AspNetCore.Mvc;
using ServiPuntosUyAdmin.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Linq;
using System.Text.Json;


namespace ServiPuntosUyAdmin.Controllers
{
    public class StationController : Controller
    {
        private readonly string apiBaseUrl = "http://localhost:5162/api/Branch";

        public async Task<IActionResult> Index()
        {
            List<Station> estaciones = new List<Station>();

            using (var client = new HttpClient())
            {
                string token = HttpContext.Session.GetString("jwt_token");
                if (!string.IsNullOrEmpty(token))
                {
                    client.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                var response = await client.GetAsync("http://localhost:5162/api/Branch");
                Console.WriteLine(">>> Response status code: " + response.StatusCode);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(">>> JSON crudo recibido del backend:");
                    Console.WriteLine(json);
                    try
                    {
                        var result = System.Text.Json.JsonSerializer.Deserialize<StationListResponse>(
                            json,
                            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                        );
                        Console.WriteLine($">>> ¿result es null? {result == null}");
                        Console.WriteLine($">>> ¿result.Data es null? {result?.Data == null}");
                        Console.WriteLine($">>> result.Data.Count: {(result?.Data != null ? result.Data.Count : -1)}");

                        if (result != null && result.Data != null)
                        {
                            foreach (var e in result.Data)
                                Console.WriteLine($"Id: {e.Id}, TenantId: {e.TenantId}, Address: {e.Address}");
                            estaciones = result.Data;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(">>> Excepción al deserializar:");
                        Console.WriteLine(ex);
                    }
                }
                else
                {
                    var json = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(">>> ERROR! Status code: " + response.StatusCode);
                    Console.WriteLine(">>> BODY: " + json);
                }
            }

            Console.WriteLine("Estaciones que se envían a la vista: " + estaciones.Count);
            foreach (var e in estaciones)
            {
                Console.WriteLine($"Id: {e.Id}, TenantId: {e.TenantId}, Address: {e.Address}");
            }

            ViewBag.Tenants = await ObtenerTenants();
            return View(estaciones);
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

        private async Task<List<Tenant>> ObtenerTenants()
        {
            var tenants = new List<Tenant>();
            using (var client = new HttpClient())
            {
                string token = HttpContext.Session.GetString("jwt_token");
                if (!string.IsNullOrEmpty(token))
                {
                    client.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                var response = await client.GetAsync("http://localhost:5162/api/Tenant");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    // Si la respuesta tiene el formato { error:..., data:[...], message:... }
                    var result = System.Text.Json.JsonSerializer.Deserialize<TenantListResponse>(json, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (result != null && result.Data != null)
                    {
                        tenants = result.Data;
                    }
                }
            }
            return tenants;
        }

    }
}
