using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System;
using Microsoft.Extensions.Configuration;
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
        private readonly string _apiBase;
        private readonly string _apiCreateTenant;
        private readonly string _apiListTenant;
        private readonly string _apiListBranch;
        private readonly string _apiCreateBranch;

        // Para LoadAvailableProducts:
        private readonly string _apiBranchProducts;
        private readonly string _apiProductList;

        public PromotionController(ILogger<PromotionController> logger, IConfiguration config)
        {
            _logger = logger;

            // Base de tu API
            var baseUrl = config["API_BASE_URL"]
                        ?? throw new InvalidOperationException("Define API_BASE_URL");

            _apiBase          = $"{baseUrl}/api/Promotion";
            _apiCreateTenant  = $"{_apiBase}/Create";
            _apiListTenant    = $"{_apiBase}/tenant";
            _apiListBranch    = $"{_apiBase}/Branch";
            _apiCreateBranch  = $"{_apiListBranch}/Create";

            // Para el helper LoadAvailableProducts
            _apiBranchProducts = $"{baseUrl}/api/Branch/products";
            _apiProductList    = $"{baseUrl}/api/Product";
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

            var resp = await client.GetAsync(_apiListBranch);
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
            var resp = await client.PostAsync(_apiCreateBranch, content);
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

            var resp = await client.GetAsync(_apiListTenant);
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
            var resp = await client.PostAsync(_apiCreateTenant, content);
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
                resp = await client.GetAsync(_apiBranchProducts);
            else
                resp = await client.GetAsync(_apiProductList);

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
