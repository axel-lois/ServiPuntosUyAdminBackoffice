using Microsoft.AspNetCore.Mvc;
using ServiPuntosUyAdmin.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ServiPuntosUyAdmin.Controllers
{
    public class TenantController : Controller
    {
        private readonly string apiBaseUrl = "http://localhost:5162/api/Tenant"; // Cambia si tu API cambia

        // GET: Tenant/Index
        public async Task<IActionResult> Index()
        {
            using (var client = new HttpClient())
            {
                // Token
                string token = HttpContext.Session.GetString("jwt_token");
                if (!string.IsNullOrEmpty(token))
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await client.GetAsync($"{apiBaseUrl}/List");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var tenants = JsonConvert.DeserializeObject<List<Tenant>>(json);
                    return View(tenants);
                }
                else
                {
                    ViewBag.Error = "No se pudo obtener la lista de cadenas.";
                    return View(new List<Tenant>());
                }
            }
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

                var response = await client.PostAsync($"{apiBaseUrl}/Create", content);

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

                var response = await client.GetAsync($"{apiBaseUrl}/{id}");
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

                // Asumiendo que la API espera { id, name }
                var json = JsonConvert.SerializeObject(new
                {
                    id = tenant.Id,
                    name = tenant.Name
                });
                var data = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PutAsync($"{apiBaseUrl}/{tenant.Id}", data);

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

                var response = await client.GetAsync($"{apiBaseUrl}/{id}");
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

                var response = await client.DeleteAsync($"{apiBaseUrl}/{id}");
                // Mensaje flash de éxito si querés
                TempData["Success"] = "¡La cadena se eliminó correctamente!";
                // Ignoramos el resultado, siempre volvemos al Index
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
