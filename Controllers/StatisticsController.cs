using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;       
using System;                           
using System.Net.Http;                    
using System.Net.Http.Headers;            
using Microsoft.Extensions.Configuration; 
using ServiPuntosUyAdmin.Models;
using System.Text.Json;

namespace ServiPuntosUyAdmin.Controllers
{
    public class StatisticsController : Controller
    {
        private readonly string _apiStatsUrl;

        public StatisticsController(IConfiguration config)
        {
            var baseUrl = config["API_BASE_URL"]
                        ?? throw new InvalidOperationException("Tienes que definir API_BASE_URL");
            _apiStatsUrl = $"{baseUrl}/api/Statistics";
        }

        public async Task<IActionResult> Users()
        {
            StatisticsData stats = null;
            using (var client = new HttpClient())
            {
                string token = HttpContext.Session.GetString("jwt_token");
                if (!string.IsNullOrEmpty(token))
                    client.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await client.GetAsync(_apiStatsUrl);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<StatisticsResponse>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (result != null && !result.Error)
                        stats = result.Data;
                }
            }
            ViewBag.Statistics = stats;
            return View();
        }

        public async Task<IActionResult> Transaction()
        {
            {
                StatisticsData stats = null;
                using (var client = new HttpClient())
                {
                    string token = HttpContext.Session.GetString("jwt_token");
                    if (!string.IsNullOrEmpty(token))
                        client.DefaultRequestHeaders.Authorization =
                            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                    var response = await client.GetAsync(_apiStatsUrl);
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        var result = JsonSerializer.Deserialize<StatisticsResponse>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        if (result != null && !result.Error)
                            stats = result.Data;
                    }
                }
                ViewBag.Statistics = stats;
                return View();
            }
        }
        public async Task<IActionResult> Offers()
        {
            {
                StatisticsData stats = null;
                using (var client = new HttpClient())
                {
                    string token = HttpContext.Session.GetString("jwt_token");
                    if (!string.IsNullOrEmpty(token))
                        client.DefaultRequestHeaders.Authorization =
                            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                    var response = await client.GetAsync(_apiStatsUrl);
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        var result = JsonSerializer.Deserialize<StatisticsResponse>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        if (result != null && !result.Error)
                            stats = result.Data;
                    }
                }
                ViewBag.Statistics = stats;
                return View();
            }
        }
        public async Task<IActionResult> Points()
        {
            {
                StatisticsData stats = null;
                using (var client = new HttpClient())
                {
                    string token = HttpContext.Session.GetString("jwt_token");
                    if (!string.IsNullOrEmpty(token))
                        client.DefaultRequestHeaders.Authorization =
                            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                    var response = await client.GetAsync(_apiStatsUrl);
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        var result = JsonSerializer.Deserialize<StatisticsResponse>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        if (result != null && !result.Error)
                            stats = result.Data;
                    }
                }
                ViewBag.Statistics = stats;
                return View();
            }
        }
    }

}