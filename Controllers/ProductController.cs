using Microsoft.AspNetCore.Mvc;
using ServiPuntosUyAdmin.Models;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ServiPuntosUyAdmin.Controllers
{
    public class ProductController : Controller
    {
        private readonly string _apiUrl = "http://localhost:5162/api/Product";

        public async Task<IActionResult> Index()
        {
            List<Product> productos = new List<Product>();

            using (var client = new HttpClient())
            {
                string token = HttpContext.Session.GetString("jwt_token");
                if (!string.IsNullOrEmpty(token))
                {
                    client.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                var response = await client.GetAsync(_apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<ProductListResponse>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (result != null && result.Data != null)
                        productos = result.Data;
                }
            }
            return View(productos);
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewData["Title"] = "Nuevo Producto";
            var userType = HttpContext.Session.GetString("user_type");
            var tenantIdStr = HttpContext.Session.GetString("tenant_id");

            var product = new Product();

            if (userType == "admin_tenant" && !string.IsNullOrEmpty(tenantIdStr))
            {
                product.TenantId = int.Parse(tenantIdStr);
            }
            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Product product)
        {
            var userType = HttpContext.Session.GetString("user_type");
            var tenantIdStr = HttpContext.Session.GetString("tenant_id");
            if (userType == "admin_tenant" && !string.IsNullOrEmpty(tenantIdStr))
            {
                product.TenantId = int.Parse(tenantIdStr);
            }

            using (var client = new HttpClient())
            {
                string token = HttpContext.Session.GetString("jwt_token");
                if (!string.IsNullOrEmpty(token))
                {
                    client.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                var json = JsonSerializer.Serialize(product, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync($"{_apiUrl}/Create", content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Producto creado exitosamente.";
                    return RedirectToAction("Index");
                }

                TempData["Error"] = "Error al crear el producto.";
                return View(product);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            Product producto = null;
            using (var client = new HttpClient())
            {
                string token = HttpContext.Session.GetString("jwt_token");
                if (!string.IsNullOrEmpty(token))
                {
                    client.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                var response = await client.GetAsync($"{_apiUrl}/{id}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var apiResult = JsonSerializer.Deserialize<ProductResponse>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (apiResult != null && apiResult.Data != null)
                        producto = apiResult.Data;
                }
            }
            if (producto == null)
            {
                TempData["Error"] = "No se pudo encontrar el producto.";
                return RedirectToAction("Index");
            }
            Console.WriteLine($"GET Edit: ProductId={producto.ProductId}, Name={producto.Name}");
            return View(producto);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Product product)
        {
            Console.WriteLine("ProductId recibido en POST Edit: " + product.ProductId);
            using (var client = new HttpClient())
            {
                string token = HttpContext.Session.GetString("jwt_token");
                if (!string.IsNullOrEmpty(token))
                {
                    client.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                var json = JsonSerializer.Serialize(product, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
                Console.WriteLine("JSON ENVIADO AL BACKEND: " + json); // <-- AGREGALO ACA

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync($"{_apiUrl}/Update", content);
                Console.WriteLine("Editando producto con ProductId: " + product.ProductId);

                var responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Status: {response.StatusCode} Body: {responseBody}");

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Producto actualizado correctamente.";
                    return RedirectToAction("Index");
                }
                TempData["Error"] = $"Error al actualizar el producto. Detalle: {responseBody}";
                return View(product);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            Product producto = null;
            using (var client = new HttpClient())
            {
                string token = HttpContext.Session.GetString("jwt_token");
                if (!string.IsNullOrEmpty(token))
                {
                    client.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                // Primero obtenemos el producto para enviar el objeto completo al endpoint Delete
                var response = await client.GetAsync($"{_apiUrl}/{id}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var apiResult = JsonSerializer.Deserialize<ProductResponse>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (apiResult != null && apiResult.Data != null)
                        producto = apiResult.Data;
                }
            }

            if (producto == null)
            {
                TempData["Error"] = "No se pudo encontrar el producto.";
                return RedirectToAction("Index");
            }

            using (var client = new HttpClient())
            {
                string token = HttpContext.Session.GetString("jwt_token");
                if (!string.IsNullOrEmpty(token))
                {
                    client.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }
                var json = JsonSerializer.Serialize(producto, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync($"{_apiUrl}/Delete", content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Producto eliminado correctamente.";
                }
                else
                {
                    TempData["Error"] = "Error al eliminar el producto.";
                }
            }

            return RedirectToAction("Index");
        }
    }
}
