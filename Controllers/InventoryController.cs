using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using ServiPuntosUyAdmin.Models;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;
using JsonNet = Newtonsoft.Json;
using System.Text.Json;
using System;
using Microsoft.Extensions.Configuration;

namespace ServiPuntosUyAdmin.Controllers
{
    public class InventoryController : Controller
    {
        private readonly string _apiBaseUrl;
        private readonly string _apiProductsUrl;
        private readonly string _apiStockUrl;

        public InventoryController(IConfiguration config)
        {
            var baseUrl = config["API_BASE_URL"]
                          ?? throw new InvalidOperationException("Tienes que definir API_BASE_URL");
            // URL para obtener productos
            _apiProductsUrl = $"{baseUrl}/api/Branch/products";
            // URL para actualizar stock
            _apiStockUrl    = $"{baseUrl}/api/Branch/stock";
        }

        // GET /Inventory
        public async Task<IActionResult> Index()
        {
            if (!int.TryParse(HttpContext.Session.GetString("branch_id"), out _))
                return RedirectToAction("Login", "Account");

            var list = new List<BranchProduct>();
            using var client = new HttpClient();
            var token = HttpContext.Session.GetString("jwt_token");
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

            var resp = await client.GetAsync(_apiProductsUrl);
            if (resp.IsSuccessStatusCode)
            {
                var json = await resp.Content.ReadAsStringAsync();
                var wrapper = JsonSerializer.Deserialize<BranchProductListResponse>(
                    json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                list = wrapper?.Data ?? new();
            }
            else
            {
                TempData["Error"] = "No se pudo cargar el inventario.";
            }

            return View(list);
        }

        [HttpGet]
        public IActionResult Create(int productId, string productName)
        {
            var vm = new InventoryStockViewModel
            {
                ProductId   = productId,
                ProductName = productName,
                Quantity    = 0
            };
            return View("Edit", vm);
        }

        [HttpGet]
        public IActionResult Edit(int productId, string productName, int quantity)
        {
            var vm = new InventoryStockViewModel
            {
                ProductId   = productId,
                ProductName = productName,
                Quantity    = quantity
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(InventoryStockViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            using var client = new HttpClient();
            var token = HttpContext.Session.GetString("jwt_token");
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

            var payload = new
            {
                productId = vm.ProductId,
                stock     = vm.Quantity
            };

            var json    = JsonNet.JsonConvert.SerializeObject(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var resp = await client.PostAsync(_apiStockUrl, content);
            if (resp.IsSuccessStatusCode)
            {
                TempData["Success"] = "Stock actualizado correctamente";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                var error = await resp.Content.ReadAsStringAsync();
                ModelState.AddModelError("", "Error al guardar stock: " + error);
                return View(vm);
            }
        }
    }
}
