using Microsoft.AspNetCore.Mvc;
using System;
using Microsoft.AspNetCore.Http;
using ServiPuntosUyAdmin.Models;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net;
using Microsoft.Extensions.Configuration;
using JsonNet       = Newtonsoft.Json;
using SystemJson    = System.Text.Json;
using System.Net.Http.Headers; 

namespace ServiPuntosUyAdmin.Controllers
{
    public class StationController : Controller
    {
        private readonly string _apiBranchUrl;
        private readonly string _apiTenantUrl;

        public StationController(IConfiguration config)
        {
            var baseUrl = config["API_BASE_URL"]
                          ?? throw new InvalidOperationException("Tienes que definir API_BASE_URL");
            _apiBranchUrl = $"{baseUrl}/api/Branch";
            _apiTenantUrl = $"{baseUrl}/api/Tenant";
        }

        // GET: Station/Index
        public async Task<IActionResult> Index()
        {
            List<Tenant> tenants = new List<Tenant>();
            var userType = HttpContext.Session.GetString("user_type");
            var stations = new List<Station>();

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

                        var response = await client.GetAsync(_apiBranchUrl);
                        if (response.IsSuccessStatusCode)
                        {
                            var json = await response.Content.ReadAsStringAsync();
                            var result = System.Text.Json.JsonSerializer.Deserialize<StationListResponse>(json, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                            if (result != null && result.Data != null)
                                stations = result.Data.Where(s => s.TenantId == tenantId).ToList();
                        }

                        var respTenant = await client.GetAsync($"{_apiTenantUrl}/{tenantId}");
                        if (respTenant.IsSuccessStatusCode)
                        {
                            var json = await respTenant.Content.ReadAsStringAsync();
                            var tenant = System.Text.Json.JsonSerializer.Deserialize<Tenant>(json, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                            if (tenant != null)
                                tenants.Add(tenant);
                        }
                    }
                }
            }
            else // admin_central
            {
                using (var client = new HttpClient())
                {
                    string token = HttpContext.Session.GetString("jwt_token");
                    if (!string.IsNullOrEmpty(token))
                        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                    var response = await client.GetAsync(_apiBranchUrl);
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        var result = System.Text.Json.JsonSerializer.Deserialize<StationListResponse>(json, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        if (result != null && result.Data != null)
                            stations = result.Data;
                    }

                    tenants = await ObtenerTenants();
                }
            }

            ViewBag.Tenants = tenants;
            return View(stations);
        }

        private async Task<List<Tenant>> ObtenerTenants()
        {
            var tenants = new List<Tenant>();
            using (var client = new HttpClient())
            {
                string token = HttpContext.Session.GetString("jwt_token");
                if (!string.IsNullOrEmpty(token))
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await client.GetAsync(_apiTenantUrl);
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
            var tenantIdStr = HttpContext.Session.GetString("tenant_id");
            System.Diagnostics.Debug.WriteLine($"[DEBUG] user_type: {userType}, tenant_id: {tenantIdStr}");

            if (userType == "admin_tenant")
            {
                if (!string.IsNullOrEmpty(tenantIdStr) && int.TryParse(tenantIdStr, out int tenantId))
                {
                    using (var client = new HttpClient())
                    {
                        string token = HttpContext.Session.GetString("jwt_token");
                        if (!string.IsNullOrEmpty(token))
                            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                        var resp = await client.GetAsync($"{_apiTenantUrl}/{tenantId}");
                        var json = await resp.Content.ReadAsStringAsync();
                        System.Diagnostics.Debug.WriteLine($"[DEBUG] Tenant API resp: {json}");

                        if (resp.IsSuccessStatusCode)
                        {
                            var tenant = System.Text.Json.JsonSerializer.Deserialize<Tenant>(json, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                            if (tenant != null)
                            {
                                tenants.Add(tenant);
                                System.Diagnostics.Debug.WriteLine($"[DEBUG] Agregado tenant: {tenant.Id} - {tenant.Name}");
                            }
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("[DEBUG] Error al obtener tenant desde API");
                        }
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("[DEBUG] tenantIdStr vacío o inválido");
                }
            }
            else // admin_central
            {
                tenants = await ObtenerTenants();
            }

            System.Diagnostics.Debug.WriteLine($"[DEBUG] Tenants count para view: {tenants.Count}");

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

                        var resp = await client.GetAsync($"{_apiTenantUrl}/{tenantId}");
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

                var response = await client.PostAsync($"{_apiBranchUrl}/Create", content);

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

                        var resp = await client.GetAsync($"{_apiTenantUrl}/{tenantId}");
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

                var response = await client.GetAsync(_apiBranchUrl);
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

                var request = new HttpRequestMessage(new HttpMethod("PATCH"), $"{_apiBranchUrl}/{station.Id}/Update")
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
            using (var client = new HttpClient())
            {
                string token = HttpContext.Session.GetString("jwt_token");
                if (!string.IsNullOrEmpty(token))
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                // Solo se puede hacer GET de todas, no por id
                var response = await client.GetAsync(_apiBranchUrl);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = System.Text.Json.JsonSerializer.Deserialize<StationListResponse>(json, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    var station = result?.Data?.FirstOrDefault(s => s.Id == id);

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

                var response = await client.DeleteAsync($"{_apiBranchUrl}/{id}/Delete");
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

        // GET: Station/Hours
        [HttpGet]
        public IActionResult Hours()
        {
            // Sólo Admin Branch
            if (!int.TryParse(HttpContext.Session.GetString("branch_id"), out var branchId))
                return RedirectToAction("Login", "Account");

            var vm = new StationHoursDto
            {
                BranchId = branchId
            };
            return View(vm);
        }

        // POST: Station/Hours
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Hours(StationHoursDto model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Armo el payload
            var payload = new
            {
                branchId    = model.BranchId,
                openTime    = model.OpenTime,
                closingTime = model.ClosingTime
            };

            using var client = new HttpClient();
            var token = HttpContext.Session.GetString("jwt_token");
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization = 
                    new AuthenticationHeaderValue("Bearer", token);

            // Serializamos y enviamos
            var json = JsonNet.JsonConvert.SerializeObject(payload);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            var resp = await client.PostAsync($"{_apiBranchUrl}/hours", content);

            // SI JWT expiró, redirige de una vez al login
            if (resp.StatusCode == HttpStatusCode.Unauthorized)
                return RedirectToAction("Login", "Account");

            if (!resp.IsSuccessStatusCode)
            {
                var error = await resp.Content.ReadAsStringAsync();
                TempData["Error"] = "No se pudieron guardar los horarios: " + error;
                return View(model);
            }

            TempData["Success"] = "Horarios actualizados correctamente";
            return RedirectToAction(nameof(Hours));
        }
    }
}
