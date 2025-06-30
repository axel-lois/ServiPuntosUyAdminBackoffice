using Microsoft.AspNetCore.Mvc;
using ServiPuntosUyAdmin.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using System;
using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;

namespace ServiPuntosUyAdmin.Controllers
{
    public class TenantController : Controller
    {
        private readonly string _apiTenantBase;
        private readonly string _apiTenantUi;
        private readonly string _apiPublicTenant;

        public TenantController(IConfiguration config)
        {
            var baseUrl = config["API_BASE_URL"]
                        ?? throw new InvalidOperationException("Tienes que definir API_BASE_URL");
            _apiTenantBase   = $"{baseUrl}/api/Tenant";
            _apiTenantUi     = $"{baseUrl}/api/TenantUI";
            _apiPublicTenant = $"{baseUrl}/api/public/tenant";
        }

        // GET: Tenant/Index
        public async Task<IActionResult> Index()
        {
            List<Tenant> tenants = new();

            using var client = new HttpClient();
            var resp = await client.GetAsync(_apiPublicTenant);
            if (resp.IsSuccessStatusCode)
            {
                var json = await resp.Content.ReadAsStringAsync();
                var wrapper = JsonConvert.DeserializeObject<ApiResponse<List<Tenant>>>(json);
                tenants = wrapper?.Data ?? new List<Tenant>();
            }
            else
            {
                ViewBag.Error = "No se pudo cargar la lista de cadenas públicas.";
            }

            return View(tenants);
        }

        // GET: Tenant/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Tenant/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Tenant tenant)
        {
            if (!ModelState.IsValid)
            {
                return View(tenant);
            }

            using (var client = new HttpClient())
            {
                // Token
                string token = HttpContext.Session.GetString("jwt_token");
                if (!string.IsNullOrEmpty(token))
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                // Solo enviamos el campo que la API espera (name)
                var json = JsonConvert.SerializeObject(new
                {
                    name = tenant.Name
                });
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync($"{_apiTenantBase}/Create", content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "¡La cadena se creó correctamente!";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ViewBag.Error = "No se pudo crear el Tenant. Verifique los datos o el acceso.";
                    return View(tenant);
                }
            }
        }

        // GET: Tenant/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            using (var client = new HttpClient())
            {
                // Token
                string token = HttpContext.Session.GetString("jwt_token");
                if (!string.IsNullOrEmpty(token))
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await client.GetAsync($"{_apiTenantBase}/{id}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var tenant = JsonConvert.DeserializeObject<Tenant>(json);
                    if (tenant == null) return NotFound();
                    return View(tenant);
                }
                else
                {
                    return NotFound();
                }
            }
        }

        // POST: Tenant/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Tenant tenant)
        {
            if (!ModelState.IsValid)
                return View(tenant);

            using (var client = new HttpClient())
            {
                // Token
                string token = HttpContext.Session.GetString("jwt_token");
                if (!string.IsNullOrEmpty(token))
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var json = JsonConvert.SerializeObject(new
                {
                    id = tenant.Id,
                    name = tenant.Name
                });
                var data = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PutAsync($"{_apiTenantBase}/{tenant.Id}", data);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "¡La cadena se editó correctamente!";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ViewBag.Error = "Error al editar la cadena.";
                    return View(tenant);
                }
            }
        }

        // GET: Tenant/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            using (var client = new HttpClient())
            {
                // Token
                string token = HttpContext.Session.GetString("jwt_token");
                if (!string.IsNullOrEmpty(token))
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await client.GetAsync($"{_apiTenantBase}/{id}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var tenant = JsonConvert.DeserializeObject<Tenant>(json);
                    if (tenant == null) return NotFound();
                    return View(tenant);
                }
                else
                {
                    return NotFound();
                }
            }
        }

        // POST: Tenant/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            using (var client = new HttpClient())
            {
                // Token
                string token = HttpContext.Session.GetString("jwt_token");
                if (!string.IsNullOrEmpty(token))
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await client.DeleteAsync($"{_apiTenantBase}/{id}");
                TempData["Success"] = "¡La cadena se eliminó correctamente!";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Tenant/Personalization/5
        public async Task<IActionResult> Personalization(int id)
        {
            TenantUIConfig config = new TenantUIConfig();
            using (var client = new HttpClient())
            {
                string? token = HttpContext.Session.GetString("jwt_token");
                if (!string.IsNullOrEmpty(token))
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var resp = await client.GetAsync($"{_apiTenantUi}/{id}");
                if (resp.IsSuccessStatusCode)
                {
                    var json = await resp.Content.ReadAsStringAsync();
                    var wrapper = JsonConvert.DeserializeObject<ApiResponse<TenantUIConfig>>(json);
                    if (wrapper is not null && wrapper.Data is not null)
                        config = wrapper.Data;
                }
            }

            ViewBag.TenantId = id;
            return View(config);
        }

        // POST: Tenant/Personalization/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Personalization(int id, TenantUIConfig config)
        {
            using (var client = new HttpClient())
            {
                string token = HttpContext.Session.GetString("jwt_token");
                if (!string.IsNullOrEmpty(token))
                    client.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var json = JsonConvert.SerializeObject(new
                {
                    LogoUrl = config.LogoUrl,
                    primaryColor = config.PrimaryColor,
                    secondaryColor = config.SecondaryColor
                });
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PutAsync($"{_apiTenantUi}/{id}", content);

                if (response.IsSuccessStatusCode)
                {
                    // En lugar de RedirectToAction, devolvemos la misma vista
                    ViewBag.Success = "¡Personalización actualizada correctamente!";
                    ViewBag.TenantId = id;
                    return View(config);
                }
                else
                {
                    ViewBag.Error = "Error al guardar la personalización.";
                    ViewBag.TenantId = id;
                    return View(config);
                }
            }
        }


        // GET: /Tenant/ListTenant
        [HttpGet]
        public async Task<IActionResult> ListTenant()
        {
            using (var client = new HttpClient())
            {
                string token = HttpContext.Session.GetString("jwt_token");
                if (!string.IsNullOrEmpty(token))
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await client.GetAsync(_apiTenantBase);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var responseObj = JsonConvert.DeserializeObject<TenantListResponse>(json);
                    if (responseObj != null && responseObj.Data != null)
                    {
                        var result = responseObj.Data.Select(t => new { t.Id, t.Name });
                        return Json(result);
                    }
                }
                return Json(new List<object>());
            }
        }
    }
}
