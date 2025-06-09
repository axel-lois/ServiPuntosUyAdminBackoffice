using Microsoft.AspNetCore.Mvc;
using ServiPuntosUyAdmin.Models;
using System.Text.Json;

namespace ServiPuntosUyAdmin.Controllers
{
    public class StatisticsController : Controller
    {
        public async Task<IActionResult> Users()
        {
            StatisticsData stats = null;
            using (var client = new HttpClient())
            {
                string token = HttpContext.Session.GetString("jwt_token");
                if (!string.IsNullOrEmpty(token))
                    client.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await client.GetAsync("http://localhost:5162/api/Statistics");
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

                    var response = await client.GetAsync("http://localhost:5162/api/Statistics");
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

                    var response = await client.GetAsync("http://localhost:5162/api/Statistics");
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

                    var response = await client.GetAsync("http://localhost:5162/api/Statistics");
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