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
        private readonly string apiBaseUrl = "http://localhost:5162/api/Tenant"; 

        // GET: Tenant/Index
        public async Task<IActionResult> Index()
        {
            using (var client = new HttpClient())
            {
                // Token
                string token = HttpContext.Session.GetString("jwt_token");
                if (!string.IsNullOrEmpty(token))
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await client.GetAsync($"{apiBaseUrl}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var responseObj = JsonConvert.DeserializeObject<TenantListResponse>(json);
                    if (responseObj != null && responseObj.Data != null)
                    {
                        return View(responseObj.Data);
                    }
                }

                ViewBag.Error = "No se pudo obtener la lista de cadenas.";
                return View(new List<Tenant>());
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

        // GET: Tenant/Personalization/5
        public async Task<IActionResult> Personalization(int id)
        {
            using (var client = new HttpClient())
            {
                string token = HttpContext.Session.GetString("jwt_token");
                if (!string.IsNullOrEmpty(token))
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await client.GetAsync($"http://localhost:5162/api/TenantUI/{id}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var config = Newtonsoft.Json.JsonConvert.DeserializeObject<TenantUIConfig>(json);
                    ViewBag.TenantId = id;
                    return View(config);
                }
                else
                {
                    ViewBag.Error = "No se pudo obtener la configuración de personalización.";
                    ViewBag.TenantId = id;
                    return View(new TenantUIConfig());
                }
            }
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
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var json = Newtonsoft.Json.JsonConvert.SerializeObject(new
                {
                    imgUrl = config.ImgUrl,
                    primaryColor = config.PrimaryColor,
                    secondaryColor = config.SecondaryColor
                });
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PutAsync($"http://localhost:5162/api/TenantUI/{id}", content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "¡Personalización actualizada!";
                    return RedirectToAction("Index");
                }
                else
                {
                    ViewBag.Error = "Error al guardar la personalización.";
                    return View(config);
                }
            }
        }
    }
}
