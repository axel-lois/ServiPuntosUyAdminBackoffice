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

        // --- Reutilizable: Obtener todos los tenants ---
        private async Task<List<Tenant>> ObtenerTenants()
        {
            var tenants = new List<Tenant>();
            using (var client = new HttpClient())
            {
                string token = HttpContext.Session.GetString("jwt_token");
                if (!string.IsNullOrEmpty(token))
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await client.GetAsync("http://localhost:5162/api/Tenant");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = System.Text.Json.JsonSerializer.Deserialize<TenantListResponse>(json, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (result != null && result.Data != null)
                        tenants = result.Data;
                }
            }
            return tenants;
        }

        // --- GET: Station/Create ---
        public async Task<IActionResult> Create()
        {
            List<Tenant> tenants = new List<Tenant>();
            var userType = HttpContext.Session.GetString("user_type");

            if (userType == "admin_tenant")
            {
                var tenantIdStr = HttpContext.Session.GetString("tenant_id");
                if (!string.IsNullOrEmpty(tenantIdStr) && int.TryParse(tenantIdStr, out int tenantId))
                {
                    using (var client = new HttpClient())
                    {
                        string token = HttpContext.Session.GetString("jwt_token");
                        if (!string.IsNullOrEmpty(token))
                            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                        var resp = await client.GetAsync($"http://localhost:5162/api/Tenant/{tenantId}");
                        if (resp.IsSuccessStatusCode)
                        {
                            var json = await resp.Content.ReadAsStringAsync();
                            var tenant = System.Text.Json.JsonSerializer.Deserialize<Tenant>(json, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                            if (tenant != null)
                                tenants.Add(tenant);
                        }
                    }
                }
            }
            else // admin_central u otro
            {
                tenants = await ObtenerTenants();
            }

            ViewBag.Tenants = tenants;
            return View(new Station
            {
                TenantId = tenants.Count == 1 ? tenants[0].Id : 0 // Preselecciona si solo hay uno
            });
        }

        // --- POST: Station/Create ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Station station)
        {
            // Recuperar tenants según tipo usuario para errores de validación también
            List<Tenant> tenants = new List<Tenant>();
            var userType = HttpContext.Session.GetString("user_type");

            if (userType == "admin_tenant")
            {
                var tenantIdStr = HttpContext.Session.GetString("tenant_id");
                if (!string.IsNullOrEmpty(tenantIdStr) && int.TryParse(tenantIdStr, out int tenantId))
                {
                    using (var client = new HttpClient())
                    {
                        string token = HttpContext.Session.GetString("jwt_token");
                        if (!string.IsNullOrEmpty(token))
                            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                        var resp = await client.GetAsync($"http://localhost:5162/api/Tenant/{tenantId}");
                        if (resp.IsSuccessStatusCode)
                        {
                            var json = await resp.Content.ReadAsStringAsync();
                            var tenant = System.Text.Json.JsonSerializer.Deserialize<Tenant>(json, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                            if (tenant != null)
                                tenants.Add(tenant);
                        }
                    }
                }
            }
            else
            {
                tenants = await ObtenerTenants();
            }

            ViewBag.Tenants = tenants;

            if (!ModelState.IsValid)
                return View(station);

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
                    ViewBag.Error = "No se pudo crear la estación. Detalles: " + apiResponse;
                    return View(station);
                }
            }
        }

        // --- GET: Station/Edit ---
        public async Task<IActionResult> Edit(int id)
        {
            List<Tenant> tenants = new List<Tenant>();
            var userType = HttpContext.Session.GetString("user_type");

            if (userType == "admin_tenant")
            {
                var tenantIdStr = HttpContext.Session.GetString("tenant_id");
                if (!string.IsNullOrEmpty(tenantIdStr) && int.TryParse(tenantIdStr, out int tenantId))
                {
                    using (var client = new HttpClient())
                    {
                        string token = HttpContext.Session.GetString("jwt_token");
                        if (!string.IsNullOrEmpty(token))
                            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                        var resp = await client.GetAsync($"http://localhost:5162/api/Tenant/{tenantId}");
                        if (resp.IsSuccessStatusCode)
                        {
                            var json = await resp.Content.ReadAsStringAsync();
                            var tenant = System.Text.Json.JsonSerializer.Deserialize<Tenant>(json, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                            if (tenant != null)
                                tenants.Add(tenant);
                        }
                    }
                }
            }
            else
            {
                tenants = await ObtenerTenants();
            }

            ViewBag.Tenants = tenants;

            Station station = null;
            using (var client = new HttpClient())
            {
                string token = HttpContext.Session.GetString("jwt_token");
                if (!string.IsNullOrEmpty(token))
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var url = $"http://localhost:5162/api/Branch";
                var response = await client.GetAsync(url);
                var json = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = System.Text.Json.JsonSerializer.Deserialize<StationListResponse>(json, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (result != null && result.Data != null)
                        station = result.Data.FirstOrDefault(e => e.Id == id);
                }
            }

            if (station == null)
            {
                TempData["Error"] = "No se pudo encontrar la estación.";
                return RedirectToAction(nameof(Index));
            }

            return View(station);
        }

        // --- POST: Station/Edit ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Station station)
        {
            // Recuperar tenants para la view en caso de error
            List<Tenant> tenants = new List<Tenant>();
            var userType = HttpContext.Session.GetString("user_type");

            if (userType == "admin_tenant")
            {
                var tenantIdStr = HttpContext.Session.GetString("tenant_id");
                if (!string.IsNullOrEmpty(tenantIdStr) && int.TryParse(tenantIdStr, out int tenantId))
                {
                    using (var client = new HttpClient())
                    {
                        string token = HttpContext.Session.GetString("jwt_token");
                        if (!string.IsNullOrEmpty(token))
                            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                        var resp = await client.GetAsync($"http://localhost:5162/api/Tenant/{tenantId}");
                        if (resp.IsSuccessStatusCode)
                        {
                            var json = await resp.Content.ReadAsStringAsync();
                            var tenant = System.Text.Json.JsonSerializer.Deserialize<Tenant>(json, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                            if (tenant != null)
                                tenants.Add(tenant);
                        }
                    }
                }
            }
            else
            {
                tenants = await ObtenerTenants();
            }

            ViewBag.Tenants = tenants;

            if (!ModelState.IsValid)
                return View(station);

            using (var client = new HttpClient())
            {
                string token = HttpContext.Session.GetString("jwt_token");
                if (!string.IsNullOrEmpty(token))
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var json = System.Text.Json.JsonSerializer.Serialize(new
                {
                    address = station.Address,
                    latitud = station.Latitud,
                    longitud = station.Longitud,
                    phone = station.Phone,
                    openTime = station.OpenTime,
                    closingTime = station.ClosingTime
                });
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var request = new HttpRequestMessage(new HttpMethod("PATCH"), $"http://localhost:5162/api/Branch/{station.Id}/Update")
                {
                    Content = content
                };
                var response = await client.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "¡La estación se editó correctamente!";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
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
