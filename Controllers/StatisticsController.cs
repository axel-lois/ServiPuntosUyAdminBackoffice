using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using ServiPuntosUyAdmin.Models;

namespace ServiPuntosUyAdmin.Controllers
{
    public class StatisticsController : Controller
    {
        private readonly string _apiStatsUrl;

        public StatisticsController(IConfiguration config)
        {
            var baseUrl = config["API_BASE_URL"]
                        ?? throw new InvalidOperationException("Define API_BASE_URL en tus variables de entorno");
            _apiStatsUrl = $"{baseUrl}/api/Statistics";
        }

        // Método compartido para llamar a la API y deserializar
        private async Task<StatisticsData> FetchStatsAsync()
        {
            using var client = new HttpClient();

            // Si hay token en sesión, lo agregamos
            var token = HttpContext.Session.GetString("jwt_token");
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

            var resp = await client.GetAsync(_apiStatsUrl);
            if (!resp.IsSuccessStatusCode)
                return new StatisticsData();

            var json = await resp.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<StatisticsResponse>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return (result != null && !result.Error && result.Data != null)
                ? result.Data
                : new StatisticsData();
        }

        public async Task<IActionResult> Users()
        {
            ViewBag.Statistics = await FetchStatsAsync();
            return View();
        }

        public async Task<IActionResult> Transaction()
        {
            ViewBag.Statistics = await FetchStatsAsync();
            return View();
        }

        public async Task<IActionResult> Offers()
        {
            ViewBag.Statistics = await FetchStatsAsync();
            return View();
        }

        public async Task<IActionResult> Points()
        {
            ViewBag.Statistics = await FetchStatsAsync();
            return View();
        }
    }
}
