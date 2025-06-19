using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ServiPuntosUyAdmin.Models;
using System.Net.Http.Headers;
using SystemTextJson = System.Text.Json;

namespace ServiPuntosUyAdmin.Controllers
{
    public class PromotionController : Controller
    {
        private readonly ILogger<PromotionController> _logger;
        private const string ApiBase         = "http://localhost:5162/api/Promotion";
        private const string ApiCreateTenant = ApiBase + "/Create";
        private const string ApiListTenant   = ApiBase + "/tenant"; // según tu último comentario
        private const string ApiListBranch     = ApiBase + "/Branch";        // Modificar cuando efectivamente tenga la API
        private const string ApiCreateBranch   = ApiBase + "/Branch/Create"; // POST para crear promo en la branch

        public PromotionController(ILogger<PromotionController> logger)
        {
            _logger = logger;
        }

        // ----------------------------------------
        // INDEX para ADMIN BRANCH
        // GET /Promotion/Branch
        // ----------------------------------------
        public async Task<IActionResult> Branch()
        {
            // 1) Validar que tengamos branch_id en sesión
            if (!int.TryParse(HttpContext.Session.GetString("branch_id"), out int branchId))
                return RedirectToAction("Login", "Account");

            List<Promotion> promos = new();
            using var client = new HttpClient();
            var token = HttpContext.Session.GetString("jwt_token");
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

            // 2) Llamada al endpoint que devuelve las promos de esta branch
            //    Asumimos que GET /api/Promotion/Branch lee el branchId del token,
            //    si tu API en cambio requiere /Branch/{branchId} ajustá aquí.
            var resp = await client.GetAsync(ApiListBranch);
            if (!resp.IsSuccessStatusCode)
            {
                TempData["Error"] = "No se pudieron obtener las promociones de la sucursal";
                return View(promos);
            }

            var json = await resp.Content.ReadAsStringAsync();
            var wrapper = SystemTextJson.JsonSerializer.Deserialize<PromotionListResponse>(
                json,
                new SystemTextJson.JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );
            promos = wrapper?.Data ?? new();
            return View(promos);
        }

        // ----------------------------------------
        // GET /Promotion/Branch/Create
        // ----------------------------------------
        [HttpGet]
        public IActionResult CreateBranch()
        {
            // Validamos branch_id y tenant_id en sesión
            if (!int.TryParse(HttpContext.Session.GetString("branch_id"), out int branchId) ||
                !int.TryParse(HttpContext.Session.GetString("tenant_id"), out int tenantId))
            {
                return RedirectToAction(nameof(Branch));
            }

            var vm = new PromotionCreateViewModel
            {
                TenantId    = tenantId,
                BranchId    = branchId,
                Description = "",
                StartDate   = DateTime.Now,
                EndDate     = DateTime.Now.AddDays(7),
                Price       = 0m,
                Branches    = new List<int> { branchId }, // pre-seleccionamos la branch actual
                Products    = new List<int>()
            };
            return View("CreateBranch", vm);
        }

        // ----------------------------------------
        // POST /Promotion/Branch/Create
        // ----------------------------------------
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateBranch(PromotionCreateViewModel vm)
        {
            if (!ModelState.IsValid)
                return View("CreateBranch", vm);

            // Preparamos el payload EXACTO que tu API espera:
            // {
            //   "tenantId": -1,
            //   "branchId": 1,         <- opcional si lo infiere del token
            //   "description": "...",
            //   "startDate": "2025-06-17T00:00:00Z",
            //   "endDate":   "2025-06-19T00:00:00Z",
            //   "product": [1],
            //   "price": 100
            // }
            var payload = new
            {
                tenantId    = vm.TenantId,
                branchId    = vm.BranchId,
                description = vm.Description,
                startDate   = vm.StartDate.ToUniversalTime().ToString("o"),
                endDate     = vm.EndDate.ToUniversalTime().ToString("o"),
                product     = vm.Products,              // lista de IDs de productos
                price       = Convert.ToInt32(vm.Price) // tu API pide int
            };

            var json = JsonConvert.SerializeObject(payload);
            _logger.LogInformation("[Promotion.CreateBranch] → {json}", json);

            using var client = new HttpClient();
            var token = HttpContext.Session.GetString("jwt_token");
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var resp = await client.PostAsync(ApiCreateBranch, content);

            if (resp.IsSuccessStatusCode)
            {
                TempData["Success"] = "Promoción de sucursal creada correctamente";
                return RedirectToAction(nameof(Branch));
            }
            else
            {
                var err = await resp.Content.ReadAsStringAsync();
                ModelState.AddModelError("", "Error al crear la promoción de sucursal: " + err);
                return View("CreateBranch", vm);
            }
        }

        // -------------------------------------------------------------
        // INDEX para ADMIN TENANT: GET /Promotion
        // -------------------------------------------------------------
        public async Task<IActionResult> Index()
        {
            if (!int.TryParse(HttpContext.Session.GetString("tenant_id"), out int tenantId))
                return RedirectToAction("Login", "Account");

            List<Promotion> promos = new();
            using var client = new HttpClient();
            var token = HttpContext.Session.GetString("jwt_token");
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

            // Llamo al endpoint que devuelve todas las promos para ese tenant
            var resp = await client.GetAsync($"{ApiListTenant}");
            if (!resp.IsSuccessStatusCode)
            {
                TempData["Error"] = "No se pudieron obtener las promociones";
                return View(promos);
            }

            var json = await resp.Content.ReadAsStringAsync();
            var wrapper = SystemTextJson.JsonSerializer.Deserialize<PromotionListResponse>(
                json,
                new SystemTextJson.JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );
            promos = wrapper?.Data ?? new();
            return View(promos);
        }

        // -------------------------------------------------------------
        // GET /Promotion/Create   (Admin Tenant)
        // -------------------------------------------------------------
        [HttpGet]
        public IActionResult Create()
        {
            if (!int.TryParse(HttpContext.Session.GetString("tenant_id"), out int tenantId))
                return RedirectToAction(nameof(Index));

            var vm = new PromotionCreateViewModel
            {
                TenantId    = tenantId,
                Description = string.Empty,
                StartDate   = DateTime.Now,
                EndDate     = DateTime.Now.AddDays(7),
                Price       = 0m,
                Branches    = new List<int>(),
                Products    = new List<int>()
            };
            return View(vm);
        }

        // -------------------------------------------------------------
        // POST /Promotion/Create   (Admin Tenant)
        // -------------------------------------------------------------
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PromotionCreateViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            // ¡Ojo! tu API espera:
            // {
            //   "tenantId": -1,
            //   "description": "...",
            //   "startDate": "2025-06-17T00:00:00Z",
            //   "endDate":   "2025-06-19T00:00:00Z",
            //   "branch":    [1],
            //   "product":   [1],
            //   "price":     100
            // }

            var payload = new
            {
                tenantId    = vm.TenantId,
                description = vm.Description,
                startDate   = vm.StartDate.ToUniversalTime().ToString("o"),
                endDate     = vm.EndDate.ToUniversalTime().ToString("o"),
                branch      = vm.Branches,   // debe venir del form (select multiple)
                product     = vm.Products,   // idem
                price       = Convert.ToInt32(vm.Price)
            };

            var json = JsonConvert.SerializeObject(payload);
            _logger.LogInformation("[Promotion.Create] JSON → API: {json}", json);

            using var client = new HttpClient();
            var token = HttpContext.Session.GetString("jwt_token");
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var resp = await client.PostAsync(ApiCreateTenant, content);

            if (resp.IsSuccessStatusCode)
            {
                TempData["Success"] = "Promoción creada correctamente";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                var err = await resp.Content.ReadAsStringAsync();
                ModelState.AddModelError(string.Empty, $"Error al crear la promoción: {err}");
                return View(vm);
            }
        }
    }
}
