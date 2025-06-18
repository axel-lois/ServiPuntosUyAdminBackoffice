using SystemJson = System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using ServiPuntosUyAdmin.Models;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using JsonNet = Newtonsoft.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;


namespace ServiPuntosUyAdmin.Controllers
{
    public class ServiceController : Controller
    {
        private readonly ILogger<ServiceController> _logger;
        private readonly string apiBase = "http://localhost:5162/api/Service";

        public ServiceController(ILogger<ServiceController> logger)
        {
            _logger = logger;
        }

        // 1) Listado de servicios de esta sucursal
        public async Task<IActionResult> Index()
        {
            // Si no hay branch_id en sesión, lo mandamos al login
            if (!int.TryParse(HttpContext.Session.GetString("branch_id"), out _))
                return RedirectToAction("Login", "Account");

            // Preparamos el cliente HTTP con el Bearer token
            using var client = new HttpClient();
            var token = HttpContext.Session.GetString("jwt_token");
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization = 
                    new AuthenticationHeaderValue("Bearer", token);

            // LLAMADA sin parámetro: /api/Service/branch
            var resp = await client.GetAsync($"{apiBase}/branch");
            if (!resp.IsSuccessStatusCode)
            {
                TempData["Error"] = "No se pudieron obtener los servicios";
                return View(new List<Service>());
            }

            var json = await resp.Content.ReadAsStringAsync();
            var wrapper = JsonSerializer.Deserialize<ServiceListResponse>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            var services = wrapper?.Data ?? new List<Service>();
            return View(services);
        }

        // 2) GET Create
        [HttpGet]
        public IActionResult Create()
        {
            if (!int.TryParse(HttpContext.Session.GetString("branch_id"), out int branchId))
                return RedirectToAction("Login", "Account");

            var vm = new ServiceCreateViewModel { BranchId = branchId };
            return View(vm);
        }

        // 3) POST Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ServiceCreateViewModel vm)
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
                name          = vm.Name,
                description   = vm.Description,
                price         = vm.Price,
                ageRestricted = vm.AgeRestricted,
                startTime     = vm.StartTime,
                endTime       = vm.EndTime
            };
            var json    = JsonNet.JsonConvert.SerializeObject(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var resp = await client.PostAsync($"{apiBase}/create", content);
            if (resp.IsSuccessStatusCode)
            {
                TempData["Success"] = "Servicio creado correctamente";
                return RedirectToAction(nameof(Index));
            }

            var error = await resp.Content.ReadAsStringAsync();
            ModelState.AddModelError("", $"Error al crear el servicio: {error}");
            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (!int.TryParse(HttpContext.Session.GetString("branch_id"), out int branchId))
                return RedirectToAction("Index");

            using var client = new HttpClient();
            var token = HttpContext.Session.GetString("jwt_token");
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var resp = await client.GetAsync($"{apiBase}/{id}");
            if (!resp.IsSuccessStatusCode)
            {
                TempData["Error"] = "No se encontró el servicio";
                return RedirectToAction(nameof(Index));
            }

            var json    = await resp.Content.ReadAsStringAsync();
            var wrapper = System.Text.Json.JsonSerializer.Deserialize<ServiceResponse>(
                json,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );
            var svc = wrapper?.Data;
            if (svc == null) return RedirectToAction(nameof(Index));

            // Map a ViewModel
            var first = svc.Availabilities?.FirstOrDefault();
            var vm = new ServiceEditViewModel {
                Id            = svc.Id,
                BranchId      = branchId,
                Name          = svc.Name,
                Description   = svc.Description,
                Price         = svc.Price,
                AgeRestricted = svc.AgeRestricted,
                StartTime     = first?.StartTime.ToString(@"hh\:mm") ?? "",
                EndTime       = first?.EndTime.ToString(@"hh\:mm") ?? ""
            };

            return View(vm);
        }


        // 5) POST Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ServiceEditViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                // Volvemos a la vista con validaciones desplegadas
                return View(vm);
            }

            using var client = new HttpClient();
            var token = HttpContext.Session.GetString("jwt_token");
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var payload = new {
                name         = vm.Name,
                description  = vm.Description,
                price        = vm.Price,
                ageRestricted= vm.AgeRestricted,
                startTime    = vm.StartTime,
                endTime      = vm.EndTime
            };
            var content = new StringContent(
                JsonNet.JsonConvert.SerializeObject(payload),
                Encoding.UTF8,
                "application/json"
            );

            var resp = await client.PutAsync($"{apiBase}/{vm.Id}", content);
            if (resp.IsSuccessStatusCode)
            {
                TempData["Success"] = "Servicio actualizado correctamente";
                return RedirectToAction(nameof(Index));
            }

            TempData["Error"] = await resp.Content.ReadAsStringAsync();
            return View(vm);
        }

        // 7) POST DeleteConfirmed
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            using var client = new HttpClient();
            var token = HttpContext.Session.GetString("jwt_token");
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var resp = await client.DeleteAsync($"{apiBase}/{id}");
            if (resp.IsSuccessStatusCode)
                TempData["Success"] = "Servicio eliminado correctamente";
            else
                TempData["Error"] = await resp.Content.ReadAsStringAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
