using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ServiPuntosUyAdmin.Models;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;                    // para JsonConvert.SerializeObject
using SystemTextJson = System.Text.Json;  // alias para System.Text.Json

namespace ServiPuntosUyAdmin.Controllers
{
    public class PromotionController : Controller
    {
        private readonly ILogger<PromotionController> _logger;

        private const string ApiBase           = "http://localhost:5162/api/Promotion";
        private const string ApiCreateTenant   = ApiBase + "/Create";
        private const string ApiListTenant     = ApiBase + "/tenant";
        private const string ApiListBranch     = ApiBase + "/Branch";        // GET promos de esta sucursal
        private const string ApiCreateBranch   = ApiBase + "/Branch/Create"; // POST crea promo para branch

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
            if (!int.TryParse(HttpContext.Session.GetString("branch_id"), out int branchId))
                return RedirectToAction("Login", "Account");

            List<Promotion> promos = new();
            using var client = new HttpClient();
            var token = HttpContext.Session.GetString("jwt_token");
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var resp = await client.GetAsync(ApiListBranch);
            if (!resp.IsSuccessStatusCode)
            {
                TempData["Error"] = "No se pudieron obtener las promociones de la sucursal";
                return View("Branch", promos);
            }

            var json = await resp.Content.ReadAsStringAsync();
            var wrapper = SystemTextJson.JsonSerializer.Deserialize<PromotionListResponse>(
                json,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );
            promos = wrapper?.Data ?? new();
            return View("Branch", promos);
        }

        // ----------------------------------------
        // GET /Promotion/Branch/Create
        // ----------------------------------------
        [HttpGet]
        public async Task<IActionResult> CreateBranch()
        {
            var userType = HttpContext.Session.GetString("user_type");
            int? branchId = null;
            int? tenantId = null;

            if (userType == "admin_branch")
            {
                if (!int.TryParse(HttpContext.Session.GetString("branch_id"), out int branchIdValue))
                    return RedirectToAction("Login", "Account");
                branchId = branchIdValue;
            }
            else if (userType == "admin_tenant")
            {
                if (!int.TryParse(HttpContext.Session.GetString("tenant_id"), out int tenantIdValue))
                    return RedirectToAction("Login", "Account");
                tenantId = tenantIdValue;
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }

            // Ahora llamás al endpoint correcto
            List<BranchProduct> productos = new();
            using (var client = new HttpClient())
            {
                var token = HttpContext.Session.GetString("jwt_token");
                if (!string.IsNullOrEmpty(token))
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                HttpResponseMessage resp;
                if (userType == "admin_branch")
                {
                    resp = await client.GetAsync("http://localhost:5162/api/Branch/products");
                }
                else // admin_tenant
                {
                    resp = await client.GetAsync("http://localhost:5162/api/product");
                }

                if (resp.IsSuccessStatusCode)
                {
                    var json = await resp.Content.ReadAsStringAsync();
                    var wrapper = System.Text.Json.JsonSerializer.Deserialize<BranchProductListResponse>(
                        json, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    productos = wrapper?.Data ?? new List<BranchProduct>();
                }
            }

            var vm = new PromotionCreateViewModel
            {
                TenantId = tenantId ?? -1,
                BranchId = branchId ?? -1,
                Products = new List<int>(),
                // llená tu lista de productos si la mostrás en un select/checkbox, etc.
                AvailableProducts = productos.Select(p => new SelectListItem
                {
                    Value = p.ProductId.ToString(),
                    Text = $"{p.Name} (Stock: {p.Stock})"
                }).ToList()
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

            var payload = new
            {
                tenantId    = vm.TenantId,
                branchId    = vm.BranchId,
                description = vm.Description,
                startDate   = vm.StartDate.ToUniversalTime().ToString("o"),
                endDate     = vm.EndDate.ToUniversalTime().ToString("o"),
                product     = vm.Products,
                price       = Convert.ToInt32(vm.Price)
            };

            var json = JsonConvert.SerializeObject(payload);
            _logger.LogInformation("[Promotion.CreateBranch] → {json}", json);

            using var client = new HttpClient();
            var token = HttpContext.Session.GetString("jwt_token");
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

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
                ModelState.AddModelError(string.Empty, "Error al crear la promoción de sucursal: " + err);
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
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var resp = await client.GetAsync(ApiListTenant);
            if (!resp.IsSuccessStatusCode)
            {
                TempData["Error"] = "No se pudieron obtener las promociones";
                return View(promos);
            }

            var json = await resp.Content.ReadAsStringAsync();
            var wrapper = SystemTextJson.JsonSerializer.Deserialize<PromotionListResponse>(
                json,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );
            promos = wrapper?.Data ?? new();
            return View(promos);
        }

        // -------------------------------------------------------------
        // GET /Promotion/Create   (Admin Tenant)
        // -------------------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> Create()
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
                Products    = new List<int>(),
                AvailableProducts = new List<SelectListItem>()
            };

            // Cargo productos para el multiselect
            using var client = new HttpClient();
            var token = HttpContext.Session.GetString("jwt_token");
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var resp = await client.GetAsync("http://localhost:5162/api/Product");
            if (resp.IsSuccessStatusCode)
            {
                var prodJson = await resp.Content.ReadAsStringAsync();
                var listResp = SystemTextJson.JsonSerializer.Deserialize<ProductListResponse>(
                    prodJson,
                    new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );
                foreach (var p in listResp.Data ?? new List<Product>())
                {
                    vm.AvailableProducts.Add(new SelectListItem
                    {
                        Value = p.Id.ToString(),
                        Text  = p.Name
                    });
                }
            }
            else
            {
                TempData["Error"] = "No se pudieron cargar los productos";
            }

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

            var payload = new
            {
                tenantId    = vm.TenantId,
                description = vm.Description,
                startDate   = vm.StartDate.ToUniversalTime().ToString("o"),
                endDate     = vm.EndDate.ToUniversalTime().ToString("o"),
                branch      = vm.Branches,
                product     = vm.Products,
                price       = Convert.ToInt32(vm.Price)
            };

            var json = JsonConvert.SerializeObject(payload);
            _logger.LogInformation("[Promotion.Create] JSON → API: {json}", json);

            using var client = new HttpClient();
            var token = HttpContext.Session.GetString("jwt_token");
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

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
