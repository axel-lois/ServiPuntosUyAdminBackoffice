using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System;  
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using ServiPuntosUyAdmin.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ServiPuntosUyAdmin.Controllers
{
    public class ParameterController : Controller
    {
        private readonly string _apiBaseUrl;

        public ParameterController(IConfiguration config)
        {
            var baseUrl = config["API_BASE_URL"]
                        ?? throw new InvalidOperationException("Tienes que definir API_BASE_URL");
            _apiBaseUrl = $"{baseUrl}/api/GeneralParameter";
        }

        // GET: /Parameter
        public async Task<IActionResult> Index()
        {
            using var client = new HttpClient();
            AttachToken(client);

            var response = await client.GetAsync(_apiBaseUrl);
            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "No se pudieron cargar los parámetros.";
                return View(new List<GeneralParameter>());
            }

            var json = await response.Content.ReadAsStringAsync();
            var apiResult = JsonConvert.DeserializeObject<ApiResponse<List<GeneralParameter>>>(json);
            return View(apiResult.Data ?? new List<GeneralParameter>());
        }

        // GET: /Parameter/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Parameter/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(GeneralParameter parameter)
        {
            if (!ModelState.IsValid)
                return View(parameter);

            using var client = new HttpClient();
            AttachToken(client);

            var payload = new
            {
                key = parameter.Key,
                value = parameter.Value,
                description = parameter.Description
            };
            var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
            var response = await client.PostAsync(_apiBaseUrl, content);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "¡Parámetro creado correctamente!";
                return RedirectToAction(nameof(Index));
            }

            var err = await response.Content.ReadAsStringAsync();
            TempData["Error"] = $"Error al crear el parámetro: {err}";
            return View(parameter);
        }

        // GET: /Parameter/Edit/{key}
        [HttpGet("Parameter/Edit/{key}")]
        public async Task<IActionResult> Edit(string key)
        {
            using var client = new HttpClient();
            AttachToken(client);

            var response = await client.GetAsync($"{_apiBaseUrl}/{key}");
            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "No se encontró el parámetro solicitado.";
                return RedirectToAction(nameof(Index));
            }

            var json = await response.Content.ReadAsStringAsync();
            var apiResult = JsonConvert.DeserializeObject<ApiResponse<GeneralParameter>>(json);

            if (apiResult.Data == null)
            {
                TempData["Error"] = "No se encontró el parámetro solicitado.";
                return RedirectToAction(nameof(Index));
            }

            return View(apiResult.Data);
        }

        // POST: /Parameter/Edit/{key}
        [HttpPost("Parameter/Edit/{key}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string key, GeneralParameter parameter)
        {
            if (!ModelState.IsValid)
                return View(parameter);

            using var client = new HttpClient();
            AttachToken(client);

            var payload = new
            {
                value = parameter.Value,
                description = parameter.Description
            };
            var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
            var response = await client.PutAsync($"{_apiBaseUrl}/{key}", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "¡Parámetro actualizado correctamente!";
                return RedirectToAction(nameof(Index));
            }

            var err = await response.Content.ReadAsStringAsync();
            TempData["Error"] = $"Error al actualizar el parámetro: {err}";
            return View(parameter);
        }

        private void AttachToken(HttpClient client)
        {
            var token = HttpContext.Session.GetString("jwt_token");
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }
    }
}
