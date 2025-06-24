using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ServiPuntosUyAdmin.Models;
using SystemTextJson = System.Text.Json;

namespace ServiPuntosUyAdmin.Controllers
{
    public class PromotionController : Controller
    {
        private readonly ILogger<PromotionController> _logger;
        private const string ApiBase = "http://localhost:5162/api/Promotion";
        private const string ApiCreateTenant = ApiBase + "/Create";
        private const string ApiListTenant = ApiBase + "/tenant";
        private const string ApiListBranch = ApiBase + "/Branch";
        private const string ApiCreateBranch = ApiBase + "/Branch/Create";

        public PromotionController(ILogger<PromotionController> logger)
        {
            _logger = logger;
        }

        // -----------------------------
        // ADMIN BRANCH LIST
        // GET /Promotion/Branch
        // -----------------------------
        public async Task<IActionResult> Branch()
        {
            if (!int.TryParse(HttpContext.Session.GetString("branch_id"), out _))
                return RedirectToAction("Login", "Account");

            var promos = new List<Promotion>();
            using var client = new HttpClient();
            AttachToken(client);

            var resp = await client.GetAsync(ApiListBranch);
            if (!resp.IsSuccessStatusCode)
            {
                TempData["Error"] = "No se pudieron obtener las promociones de la sucursal.";
                return View(promos);
            }

            var json = await resp.Content.ReadAsStringAsync();
            var wrapper = SystemTextJson.JsonSerializer.Deserialize<PromotionListResponse>(
                json,
                new SystemTextJson.JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );
            promos = wrapper?.Data ?? new List<Promotion>();
            return View(promos);
        }

        // -----------------------------
        // ADMIN BRANCH CREATE
        // GET /Promotion/Branch/Create
        // -----------------------------
        [HttpGet]
        public async Task<IActionResult> CreateBranch()
        {
            var userType = HttpContext.Session.GetString("user_type");
            if (userType != "admin_branch")
                return RedirectToAction("Login", "Account");

            var branchId = int.Parse(HttpContext.Session.GetString("branch_id")!);
            var vm = new PromotionCreateViewModel
            {
                TenantId = -1,
                BranchId = branchId,
                Products = new List<int>(),
                AvailableProducts = new List<SelectListItem>(),
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(7),
                Price = 0m
            };
            await LoadAvailableProducts(vm, isBranch: true);
            return View("CreateBranch", vm);
        }

        // -----------------------------
        // ADMIN BRANCH CREATE (POST)
        // POST /Promotion/Branch/Create
        // -----------------------------
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateBranch(PromotionCreateViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                await LoadAvailableProducts(vm, isBranch: true);
                return View("CreateBranch", vm);
            }

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

            var json    = JsonConvert.SerializeObject(payload);
            _logger.LogInformation("[Promotion.CreateBranch] JSON → {json}", json);

            using var client = new HttpClient();
            AttachToken(client);

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var resp    = await client.PostAsync(ApiCreateBranch, content);
            var body    = await resp.Content.ReadAsStringAsync();

            if (resp.IsSuccessStatusCode)
            {
                TempData["Success"] = "Promoción de sucursal creada correctamente";
                return RedirectToAction(nameof(Branch));
            }

            var friendly = ExtractFriendlyMessage(body, "Error al crear la promoción de sucursal");
            ModelState.AddModelError(string.Empty, friendly);
            await LoadAvailableProducts(vm, isBranch: true);
            return View("CreateBranch", vm);
        }

        // -----------------------------
        // ADMIN TENANT LIST
        // GET /Promotion
        // -----------------------------
        public async Task<IActionResult> Index()
        {
            if (!int.TryParse(HttpContext.Session.GetString("tenant_id"), out int tenantId))
                return RedirectToAction("Login", "Account");

            var promos = new List<Promotion>();
            using var client = new HttpClient();
            AttachToken(client);

            var resp = await client.GetAsync(ApiListTenant);
            if (!resp.IsSuccessStatusCode)
            {
                TempData["Error"] = "No se pudieron obtener las promociones.";
                return View(promos);
            }

            var json = await resp.Content.ReadAsStringAsync();
            var wrapper = SystemTextJson.JsonSerializer.Deserialize<PromotionListResponse>(
                json,
                new SystemTextJson.JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );
            promos = wrapper?.Data ?? new List<Promotion>();
            return View(promos);
        }

        // -----------------------------
        // ADMIN TENANT CREATE
        // GET /Promotion/Create
        // -----------------------------
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            if (!int.TryParse(HttpContext.Session.GetString("tenant_id"), out int tenantId))
                return RedirectToAction("Login", "Account");

            var vm = new PromotionCreateViewModel
            {
                TenantId = tenantId,
                Branches = new List<int>(),
                Products = new List<int>(),
                AvailableProducts = new List<SelectListItem>(),
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(7),
                Price = 0m
            };
            await LoadAvailableProducts(vm, isBranch: false);
            return View(vm);
        }

        // -----------------------------
        // ADMIN TENANT CREATE (POST)
        // POST /Promotion/Create
        // -----------------------------
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PromotionCreateViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                await LoadAvailableProducts(vm, isBranch: false);
                return View(vm);
            }

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
            _logger.LogInformation("[Promotion.Create] JSON → {json}", json);

            using var client = new HttpClient();
            AttachToken(client);

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var resp    = await client.PostAsync(ApiCreateTenant, content);
            var body    = await resp.Content.ReadAsStringAsync();

            if (resp.IsSuccessStatusCode)
            {
                TempData["Success"] = "Promoción creada correctamente";
                return RedirectToAction(nameof(Index));
            }

            var friendly = ExtractFriendlyMessage(body, "Error al crear la promoción");
            ModelState.AddModelError(string.Empty, friendly);
            await LoadAvailableProducts(vm, isBranch: false);
            return View(vm);
        }

        // -----------------------------
        // Helpers
        // -----------------------------
        private void AttachToken(HttpClient client)
        {
            var token = HttpContext.Session.GetString("jwt_token");
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
        }

        private async Task LoadAvailableProducts(PromotionCreateViewModel vm, bool isBranch)
        {
            using var client = new HttpClient();
            AttachToken(client);

            HttpResponseMessage resp;
            if (isBranch)
                resp = await client.GetAsync("http://localhost:5162/api/Branch/products");
            else
                resp = await client.GetAsync("http://localhost:5162/api/Product");

            vm.AvailableProducts = new List<SelectListItem>();
            if (!resp.IsSuccessStatusCode) return;

            var json = await resp.Content.ReadAsStringAsync();
            if (isBranch)
            {
                var wrapper = SystemTextJson.JsonSerializer.Deserialize<BranchProductListResponse>(
                    json,
                    new SystemTextJson.JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );
                foreach (var p in wrapper?.Data ?? Enumerable.Empty<BranchProduct>())
                {
                    if (p.Stock > 0)
                        vm.AvailableProducts.Add(new SelectListItem
                        {
                            Value = p.ProductId.ToString(),
                            Text  = $"{p.Name} (Stock: {p.Stock})"
                        });
                }
            }
            else
            {
                var wrapper = SystemTextJson.JsonSerializer.Deserialize<ProductListResponse>(
                    json,
                    new SystemTextJson.JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );
                foreach (var p in wrapper?.Data ?? Enumerable.Empty<Product>())
                {
                    vm.AvailableProducts.Add(new SelectListItem
                    {
                        Value = p.Id.ToString(),
                        Text  = p.Name
                    });
                }
            }
        }

        private string ExtractFriendlyMessage(string jsonBody, string defaultMsg)
        {
            try
            {
                var apiRes = JsonConvert.DeserializeObject<ApiResponse<object>>(jsonBody);
                return apiRes?.Message ?? defaultMsg;
            }
            catch
            {
                return defaultMsg;
            }
        }
    }
}
