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
                var response = await client.GetAsync("http://localhost:5162/api/Product");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    productos = JsonSerializer.Deserialize<List<Product>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
            }
            return View(productos);
        }

        public async Task<List<Tenant>> ObtenerTenants()
        {
            List<Tenant> tenants = new List<Tenant>();
            using (var client = new HttpClient())
            {
                // Si tenés un endpoint GET /api/Tenant que devuelve todos
                var response = await client.GetAsync("http://localhost:5162/api/Tenant");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    tenants = JsonSerializer.Deserialize<List<Tenant>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
            }
            return tenants;
        }


        // GET: Mostrar formulario de creación
        [HttpGet]
        public IActionResult Create()
        {
            ViewData["Title"] = "Nuevo Producto";
            return View();
        }

        // POST: Crear producto
        [HttpPost]
        public async Task<IActionResult> Create(Product product)
        {
            using (var client = new HttpClient())
            {
                var json = JsonSerializer.Serialize(product);
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

        public async Task<IActionResult> Edit(int id)
        {
            Product producto = null;
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync($"http://localhost:5162/api/Product/{id}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    producto = JsonSerializer.Deserialize<Product>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
            }
            if (producto == null)
            {
                return NotFound();
            }
            return View(producto);
        }

        // POST: /Product/Edit
        [HttpPost]
        public async Task<IActionResult> Edit(Product product)
        {
            using (var client = new HttpClient())
            {
                var json = JsonSerializer.Serialize(product);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("http://localhost:5162/api/Product/Update", content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Producto actualizado correctamente.";
                    return RedirectToAction("Index");
                }
                TempData["Error"] = "Error al actualizar el producto.";
                return View(product);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            Product producto = null;

            // Primero obtenemos el producto a eliminar (para obtener todos los datos necesarios)
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync($"http://localhost:5162/api/Product/{id}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    producto = JsonSerializer.Deserialize<Product>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
            }

            if (producto == null)
            {
                TempData["Error"] = "No se pudo encontrar el producto.";
                return RedirectToAction("Index");
            }

            // POST al endpoint Delete
            using (var client = new HttpClient())
            {
                var json = JsonSerializer.Serialize(producto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("http://localhost:5162/api/Product/Delete", content);

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
