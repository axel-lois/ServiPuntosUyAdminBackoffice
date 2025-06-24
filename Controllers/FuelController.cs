using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using ServiPuntosUyAdmin.Models;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json;
using System.Globalization;


namespace ServiPuntosUyAdmin.Controllers
{
    public class FuelController : Controller
    {
        private readonly string apiBase = "http://localhost:5162/api/Fuel";

        // GET: Fuel/Index
        public async Task<IActionResult> Index()
        {
            // 1) obtenemos branchId de sesión
            if (!int.TryParse(HttpContext.Session.GetString("branch_id"), out int branchId))
                return RedirectToAction("Login", "Account");

            // 2) preparamos cliente
            using var client = new HttpClient();
            var token = HttpContext.Session.GetString("jwt_token");
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // 3) llamamos a GET /api/Fuel/{branchId}/prices
            var resp = await client.GetAsync($"{apiBase}/{branchId}/prices");
            if (!resp.IsSuccessStatusCode)
            {
                TempData["Error"] = "No se pudieron cargar los precios de combustibles.";
                return View(new List<FuelPriceViewModel>());
            }

            var json = await resp.Content.ReadAsStringAsync();
            var wrapper = JsonSerializer.Deserialize<WrapperList<FuelPriceViewModel>>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            var list = wrapper?.Data ?? new List<FuelPriceViewModel>();
            return View(list);
        }

        // GET: Fuel/Edit/1
        [HttpGet]
        public async Task<IActionResult> Edit(int id /* fuelType */)
        {
            if (!int.TryParse(HttpContext.Session.GetString("branch_id"), out int branchId))
                return RedirectToAction("Login", "Account");

            using var client = new HttpClient();
            var token = HttpContext.Session.GetString("jwt_token");
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // GET /api/Fuel/{branchId}/price/{fuelType}
            var resp = await client.GetAsync($"{apiBase}/{branchId}/price/{id}");
            if (!resp.IsSuccessStatusCode)
            {
                TempData["Error"] = "No se encontró el precio para ese combustible.";
                return RedirectToAction(nameof(Index));
            }

            var json = await resp.Content.ReadAsStringAsync();
            var wrapper = JsonSerializer.Deserialize<WrapperItem<FuelPriceViewModel>>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );
            var vm = wrapper?.Data;
            if (vm == null) return RedirectToAction(nameof(Index));

            vm.FuelType = id;
            return View(vm);
        }

        // POST: Fuel/Edit/1
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, FuelPriceViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            // obtenemos branchId de la sesión
            if (!int.TryParse(HttpContext.Session.GetString("branch_id"), out int branchId))
                return RedirectToAction("Login", "Account");

            // Leemos el valor bruto que el usuario ingresó
            var formPrice = Request.Form["Price"].ToString().Trim();

            // Normalizamos tanto "," como "." a punto
            // para luego parsear con InvariantCulture
            var normalized = formPrice
                .Replace(",", ".")
                // elimina espacios, si hubiera
                .Replace(" ", "");

            if (!decimal.TryParse(
                    normalized,
                    NumberStyles.AllowDecimalPoint,
                    CultureInfo.InvariantCulture,
                    out decimal parsedPrice))
            {
                ModelState.AddModelError("Price", "Precio inválido. Usa formato 50.00 o 50,00");
                return View(vm);
            }

            // Ahora armamos el contenido según lo espera tu API (puro número con punto)
            var priceRaw = parsedPrice.ToString(CultureInfo.InvariantCulture);
            var content  = new StringContent(priceRaw, Encoding.UTF8, "application/json");

            using var client = new HttpClient();
            var token = HttpContext.Session.GetString("jwt_token");
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var resp = await client.PutAsync(
                $"{apiBase}/{branchId}/price/{id}",
                content
            );

            if (resp.IsSuccessStatusCode)
            {
                TempData["Success"] = "Precio actualizado correctamente";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                var error = await resp.Content.ReadAsStringAsync();
                ModelState.AddModelError("", $"Error al guardar: {error}");
                return View(vm);
            }
        }

        // helper para deserializar array
        private class WrapperList<T>
        {
            public bool Error { get; set; }
            public List<T> Data { get; set; }
            public string Message { get; set; }
        }
        // helper para deserializar un solo item
        private class WrapperItem<T>
        {
            public bool Error { get; set; }
            public T Data { get; set; }
            public string Message { get; set; }
        }
    }
}
