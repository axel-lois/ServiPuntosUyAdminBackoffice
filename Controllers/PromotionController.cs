using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ServiPuntosUyAdmin.Models;

namespace ServiPuntosUyAdmin.Controllers
{
    public class PromotionController : Controller
    {
        private readonly string apiBase = "http://localhost:5162/api/Promotion";

        // 1) LISTAR promociones de esta branch
        public async Task<IActionResult> Index()
        {
            if (!int.TryParse(HttpContext.Session.GetString("branch_id"), out _))
                return RedirectToAction("Login", "Account");

            using var client = new HttpClient();
            var token = HttpContext.Session.GetString("jwt_token");
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var resp = await client.GetAsync($"{apiBase}/branch");
            if (!resp.IsSuccessStatusCode)
            {
                TempData["Error"] = "No se pudieron obtener las promociones";
                return View(new List<Promotion>());
            }

            var json = await resp.Content.ReadAsStringAsync();

            // Deserializamos con System.Text.Json
            var list = System.Text.Json.JsonSerializer.Deserialize<PromotionListResponse>(
                json,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            var promos = list?.Data ?? new List<Promotion>();
            return View(promos);
        }

        // 2) GET Create
        [HttpGet]
        public IActionResult Create()
        {
            if (!int.TryParse(HttpContext.Session.GetString("branch_id"), out int branchId) ||
                !int.TryParse(HttpContext.Session.GetString("tenant_id"), out int tenantId))
            {
                return RedirectToAction("Index");
            }

            var vm = new PromotionCreateViewModel
            {
                BranchId  = branchId,
                TenantId  = tenantId,
                StartDate = DateTime.Now,
                EndDate   = DateTime.Now.AddDays(7),
                Price     = 0
            };
            return View(vm);
        }

        // 3) POST Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PromotionCreateViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            using var client = new HttpClient();
            var token = HttpContext.Session.GetString("jwt_token");
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var payload = new
            {
                tenantId    = vm.TenantId,
                branchId    = vm.BranchId,
                description = vm.Description,
                startDate   = vm.StartDate,
                endDate     = vm.EndDate,
                price       = vm.Price
            };

            // Serializamos con Newtonsoft.Json
            var body = Newtonsoft.Json.JsonConvert.SerializeObject(payload);
            var content = new StringContent(body, Encoding.UTF8, "application/json");

            var resp = await client.PostAsync($"{apiBase}/branch/create", content);
            if (resp.IsSuccessStatusCode)
            {
                TempData["Success"] = "Promoción creada correctamente";
                return RedirectToAction(nameof(Index));
            }

            var error = await resp.Content.ReadAsStringAsync();
            ModelState.AddModelError(string.Empty, error);
            return View(vm);
        }

        // 4) DETAILS (opcional)
        public async Task<IActionResult> Details(int id)
        {
            if (!int.TryParse(HttpContext.Session.GetString("branch_id"), out int branchId))
                return RedirectToAction("Index");

            using var client = new HttpClient();
            var token = HttpContext.Session.GetString("jwt_token");
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // GET /api/Promotion/{id}/{branchId}
            var resp = await client.GetAsync($"{apiBase}/{id}/{branchId}");
            if (!resp.IsSuccessStatusCode)
            {
                TempData["Error"] = "No se encontró la promoción";
                return RedirectToAction(nameof(Index));
            }

            var json    = await resp.Content.ReadAsStringAsync();
            var wrapper = System.Text.Json.JsonSerializer.Deserialize<PromotionResponse>(
                json,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            var promo = wrapper?.Data;
            if (promo == null)
            {
                TempData["Error"] = "Formato de respuesta inválido";
                return RedirectToAction(nameof(Index));
            }

            return View(promo);
        }
    }
}
