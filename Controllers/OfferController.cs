using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ServiPuntosUyAdmin.Models;
using Newtonsoft.Json;
using ServiPuntosUyAdmin.Models;
 using Microsoft.AspNetCore.Mvc;
 using Microsoft.AspNetCore.Http;
 using ServiPuntosUyAdmin.Models;
 using Newtonsoft.Json;

namespace ServiPuntosUyAdmin.Controllers
{
    public class OfferController : Controller
    {
        private readonly string _apiBaseUrl;

        public OfferController(IConfiguration config)
        {
            var baseUrl = config["API_BASE_URL"]
                          ?? throw new InvalidOperationException("Tienes que definir API_BASE_URL");
            _apiBaseUrl = $"{baseUrl}/api/Promotion";
        }

        // GET: /Offer/Index
        public async Task<IActionResult> Index()
        {
            // Valido sesión y branch
            if (!int.TryParse(HttpContext.Session.GetString("branch_id"), out _))
                return RedirectToAction("Login", "Account");

            // GET /api/Promotion/branch
            using var client = new HttpClient();
            var token = HttpContext.Session.GetString("jwt_token");
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var resp = await client.GetAsync($"{_apiBaseUrl}/branch");
            if (!resp.IsSuccessStatusCode)
            {
                TempData["Error"] = "No se pudieron obtener las ofertas";
                return View(new List<Promotion>());
            }

            var json = await resp.Content.ReadAsStringAsync();
            var wrapper = System.Text.Json.JsonSerializer.Deserialize<PromotionListResponse>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            var ofertas = wrapper?.Data ?? new List<Promotion>();
            return View(ofertas);
        }
    }
}
