using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using ServiPuntosUyAdmin.Models;

namespace ServiPuntosUyAdmin.Controllers
{
    public class HomeController : Controller
    {
        private readonly string _statsUrl;
        private readonly string _historyUrl;

        // Inyectamos IConfiguration para leer API_BASE_URL
        public HomeController(IConfiguration config)
        {
            var baseUrl = config["API_BASE_URL"]
                          ?? throw new InvalidOperationException("Define API_BASE_URL en las Env Vars");

            // Construimos las URLs completas usando la variable de entorno
            _statsUrl   = $"{baseUrl}/api/Statistics";
            _historyUrl = $"{baseUrl}/api/Transaction/history";
        }

        public async Task<IActionResult> Index()
        {
            // Estadísticas existentes 
            int usuarios       = 0;
            int transacciones  = 0;
            int ofertasActivas = 0;
            int puntosCanjeados = 0;

            using (var client = new HttpClient())
            {
                // Si tenemos token en sesión, lo agregamos al header
                var token = HttpContext.Session.GetString("jwt_token");
                if (!string.IsNullOrEmpty(token))
                    client.DefaultRequestHeaders.Authorization = 
                        new AuthenticationHeaderValue("Bearer", token);

                // Ahora llamamos a la URL de producción en Railway
                var resp = await client.GetAsync(_statsUrl);
                if (resp.IsSuccessStatusCode)
                {
                    var json   = await resp.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<StatisticsResponse>(json);
                    if (result != null && !result.Error && result.Data != null)
                    {
                        usuarios      = result.Data.Users?.Total        ?? 0;
                        transacciones = result.Data.Transactions?.Total ?? 0;
                        ofertasActivas= result.Data.Promotions?.Total   ?? 0;
                        puntosCanjeados = result.Data.Transactions?.TotalPointsRedeemed ?? 0;
                    }
                }
            }

            ViewBag.Usuarios      = usuarios;
            ViewBag.Transacciones  = transacciones;
            ViewBag.OfertasActivas = ofertasActivas;
            ViewBag.PuntosCanjeados = puntosCanjeados;

            // Historial de transacciones 
            var history = new List<TransactionHistoryDto>();
            using (var client = new HttpClient())
            {
                var token = HttpContext.Session.GetString("jwt_token");
                if (!string.IsNullOrEmpty(token))
                    client.DefaultRequestHeaders.Authorization = 
                        new AuthenticationHeaderValue("Bearer", token);

                var resp = await client.GetAsync(_historyUrl);
                if (resp.IsSuccessStatusCode)
                {
                    var json    = await resp.Content.ReadAsStringAsync();
                    var wrapper = JsonConvert.DeserializeObject<TransactionHistoryResponse>(json);
                    if (wrapper != null && !wrapper.Error && wrapper.Data != null)
                        history = wrapper.Data;
                }
            }

            // Datos para la gráfica de los últimos 6 meses
            var hoy    = DateTime.UtcNow;
            var labels = Enumerable.Range(0, 6)
                                   .Select(i => hoy.AddMonths(-5 + i).ToString("MMM"))
                                   .ToArray();

            var dataTrans = labels.Select((lbl, i) => {
                var mes = hoy.AddMonths(-5 + i);
                return history.Count(h => 
                    h.CreatedAt.Month == mes.Month && h.CreatedAt.Year == mes.Year);
            }).ToArray();

            var dataPuntos = labels.Select((lbl, i) => {
                var mes = hoy.AddMonths(-5 + i);
                return history
                    .Where(h => h.CreatedAt.Month == mes.Month && h.CreatedAt.Year == mes.Year)
                    .Sum(h => h.PointsSpent);
            }).ToArray();

            ViewBag.ChartLabels       = JsonConvert.SerializeObject(labels);
            ViewBag.TransaccionesData = JsonConvert.SerializeObject(dataTrans);
            ViewBag.PuntosData        = JsonConvert.SerializeObject(dataPuntos);

            return View();
        }
    }
}
