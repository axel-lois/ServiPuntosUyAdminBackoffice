using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using ServiPuntosUyAdmin.Models;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using JsonNet = Newtonsoft.Json;
using SystemJson = System.Text.Json;

namespace ServiPuntosUyAdmin.Controllers
{
    public class ServiceController : Controller
    {
        private readonly string apiBase = "http://localhost:5162/api/Service";

        // 1) Listado de servicios de esta sucursal
        public async Task<IActionResult> Index()
        {
            // Solo branch --> tomar branch_id de sesión
            if (!int.TryParse(HttpContext.Session.GetString("branch_id"), out int branchId))
                return RedirectToAction("Login", "Account");

            List<Service> services = new();
            using var client = new HttpClient();
            // header auth
            var token = HttpContext.Session.GetString("jwt_token");
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var resp = await client.GetAsync($"{apiBase}/branch/{branchId}");
            if (resp.IsSuccessStatusCode)
            {
                var json = await resp.Content.ReadAsStringAsync();
                var list = SystemJson.JsonSerializer.Deserialize<ServiceListResponse>(
                    json,
                    new SystemJson.JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );
                services = list?.Data ?? new();
            }
            return View(services);
        }

        // 2) GET Create
        [HttpGet]
        public IActionResult Create()
        {
            if (!int.TryParse(HttpContext.Session.GetString("branch_id"), out int branchId))
                return RedirectToAction("Index");
            // preponemos branchId
            var svc = new Service { BranchId = branchId };
            return View(svc);
        }

        // 3) POST Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Service model)
        {
            if (!ModelState.IsValid)
                return View(model);

            using var client = new HttpClient();
            var token = HttpContext.Session.GetString("jwt_token");
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var payload = new {
                branchId     = model.BranchId,
                name         = model.Name,
                description  = model.Description,
                price        = model.Price,
                ageRestricted= model.AgeRestricted,
                startTime    = model.StartTime,
                endTime      = model.EndTime
            };
            var content = new StringContent(
                JsonNet.JsonConvert.SerializeObject(payload),
                Encoding.UTF8,
                "application/json"
            );

            var resp = await client.PostAsync($"{apiBase}/create", content);
            if (resp.IsSuccessStatusCode)
            {
                TempData["Success"] = "Servicio creado correctamente";
                return RedirectToAction(nameof(Index));
            }
            TempData["Error"] = await resp.Content.ReadAsStringAsync();
            return View(model);
        }

        // 4) GET Edit/{id}
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            using var client = new HttpClient();
            var token = HttpContext.Session.GetString("jwt_token");
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var resp = await client.GetAsync($"{apiBase}/{id}");
            if (!resp.IsSuccessStatusCode)
            {
                TempData["Error"] = "No se encontró el servicio";
                return RedirectToAction(nameof(Index));
            }

            var json = await resp.Content.ReadAsStringAsync();
            var svc = SystemJson.JsonSerializer.Deserialize<Service>(
                json,
                new SystemJson.JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );
            return View(svc);
        }

        // 5) POST Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Service model)
        {
            if (!ModelState.IsValid)
                return View(model);

            using var client = new HttpClient();
            var token = HttpContext.Session.GetString("jwt_token");
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var payload = new {
                name         = model.Name,
                description  = model.Description,
                price        = model.Price,
                ageRestricted= model.AgeRestricted,
                startTime    = model.StartTime,
                endTime      = model.EndTime
            };
            var content = new StringContent(
                JsonNet.JsonConvert.SerializeObject(payload),
                Encoding.UTF8,
                "application/json"
            );

            var resp = await client.PutAsync($"{apiBase}/{model.Id}", content);
            if (resp.IsSuccessStatusCode)
            {
                TempData["Success"] = "Servicio actualizado correctamente";
                return RedirectToAction(nameof(Index));
            }
            TempData["Error"] = await resp.Content.ReadAsStringAsync();
            return View(model);
        }

        // 6) GET Confirm Delete
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            // Reutiliza GET /api/Service/{id}
            using var client = new HttpClient();
            var token = HttpContext.Session.GetString("jwt_token");
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var resp = await client.GetAsync($"{apiBase}/{id}");
            if (!resp.IsSuccessStatusCode) return RedirectToAction(nameof(Index));

            var json = await resp.Content.ReadAsStringAsync();
            var svc = SystemJson.JsonSerializer.Deserialize<Service>(
                json,
                new SystemJson.JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );
            return View(svc);
        }

        // 7) POST DeleteConfirmed
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            using var client = new HttpClient();
            var token = HttpContext.Session.GetString("jwt_token");
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var resp = await client.DeleteAsync($"{apiBase}/{id}");
            if (resp.IsSuccessStatusCode)
                TempData["Success"] = "Servicio eliminado correctamente";
            else
                TempData["Error"] = await resp.Content.ReadAsStringAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
