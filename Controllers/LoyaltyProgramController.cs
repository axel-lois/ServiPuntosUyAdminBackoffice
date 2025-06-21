using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json;
using ServiPuntosUyAdmin.Models;

namespace ServiPuntosUyAdmin.Controllers
{
    public class LoyaltyProgramController : Controller
    {
        private readonly string apiBase = "http://localhost:5162/api/LoyaltyProgram";

        // GET: LoyaltyProgram/Index
        public async Task<IActionResult> Index()
        {
            if (!int.TryParse(HttpContext.Session.GetString("tenant_id"), out int tenantId))
                return RedirectToAction("Login", "Account");

            using var client = new HttpClient();
            var token = HttpContext.Session.GetString("jwt_token");
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var resp = await client.GetAsync($"{apiBase}/{tenantId}/program");
            if (!resp.IsSuccessStatusCode)
            {
                TempData["Error"] = "No se pudo obtener el programa de fidelidad.";
                return View(new LoyaltyProgramViewModel());
            }

            var json = await resp.Content.ReadAsStringAsync();
            var wrapper = JsonSerializer.Deserialize<WrapperItem<LoyaltyProgramViewModel>>(
                json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return View(wrapper?.Data ?? new LoyaltyProgramViewModel());
        }

        // GET: LoyaltyProgram/Create
        public IActionResult Create()
        {
            return View(new LoyaltyProgramViewModel());
        }

        // POST: LoyaltyProgram/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LoyaltyProgramViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            using var client = new HttpClient();
            var token = HttpContext.Session.GetString("jwt_token");
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var payload = JsonSerializer.Serialize(vm);
            var content = new StringContent(payload, Encoding.UTF8, "application/json");

            var resp = await client.PostAsync(apiBase, content);
            if (resp.IsSuccessStatusCode)
            {
                TempData["Success"] = "Programa creado correctamente";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                var error = await resp.Content.ReadAsStringAsync();
                ModelState.AddModelError("", $"Error al crear: {error}");
                return View(vm);
            }
        }

        // GET: LoyaltyProgram/Edit
        public async Task<IActionResult> Edit()
        {
            if (!int.TryParse(HttpContext.Session.GetString("tenant_id"), out int tenantId))
                return RedirectToAction("Login", "Account");

            using var client = new HttpClient();
            var token = HttpContext.Session.GetString("jwt_token");
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var resp = await client.GetAsync($"{apiBase}/{tenantId}/program");
            if (!resp.IsSuccessStatusCode)
            {
                TempData["Error"] = "No se pudo obtener el programa para editar.";
                return RedirectToAction(nameof(Index));
            }

            var json = await resp.Content.ReadAsStringAsync();
            var wrapper = JsonSerializer.Deserialize<WrapperItem<LoyaltyProgramViewModel>>(
                json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return View(wrapper?.Data ?? new LoyaltyProgramViewModel());
        }

        // POST: LoyaltyProgram/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(LoyaltyProgramViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            using var client = new HttpClient();
            var token = HttpContext.Session.GetString("jwt_token");
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var payload = JsonSerializer.Serialize(vm);
            var content = new StringContent(payload, Encoding.UTF8, "application/json");

            var resp = await client.PutAsync(apiBase, content);
            if (resp.IsSuccessStatusCode)
            {
                TempData["Success"] = "Programa editado correctamente";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                var error = await resp.Content.ReadAsStringAsync();
                ModelState.AddModelError("", $"Error al editar: {error}");
                return View(vm);
            }
        }

        // Helpers para deserializar
        private class WrapperItem<T>
        {
            public bool Error { get; set; }
            public T Data { get; set; }
            public string Message { get; set; }
        }
    }
}
