using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ServiPuntosUyAdmin.Models;

public class HomeController : Controller
{
    private const string STATS_URL   = "http://localhost:5162/api/Statistics";
    private const string HISTORY_URL = "http://localhost:5162/api/Transaction/history";

    public async Task<IActionResult> Index()
    {
        //  Estadísticas existentes 
        int usuarios       = 0;
        int transacciones  = 0;
        int ofertasActivas = 0;

        using (var client = new HttpClient())
        {
            var token = HttpContext.Session.GetString("jwt_token");
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var resp = await client.GetAsync(STATS_URL);
            if (resp.IsSuccessStatusCode)
            {
                var json   = await resp.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<StatisticsResponse>(json);

                if (result != null && !result.Error && result.Data != null)
                {
                    usuarios      = result.Data.Users?.Total        ?? 0;
                    transacciones = result.Data.Transactions?.Total ?? 0;
                    ofertasActivas= result.Data.Promotions?.Total   ?? 0;
                }
            }
        }

        ViewBag.Usuarios      = usuarios;
        ViewBag.Transacciones  = transacciones;
        ViewBag.OfertasActivas = ofertasActivas;

        // Historial de transacciones 
        var history = new List<TransactionHistoryDto>();
        using (var client = new HttpClient())
        {
            var token = HttpContext.Session.GetString("jwt_token");
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var resp = await client.GetAsync(HISTORY_URL);
            if (resp.IsSuccessStatusCode)
            {
                var json    = await resp.Content.ReadAsStringAsync();
                var wrapper = JsonConvert.DeserializeObject<TransactionHistoryResponse>(json);
                if (wrapper != null && !wrapper.Error && wrapper.Data != null)
                    history = wrapper.Data;
            }
        }

        // Total de puntos canjeados (sumamos pointsSpent) 
        var puntosCanjeados = history.Sum(h => h.PointsSpent);
        ViewBag.PuntosCanjeados = puntosCanjeados;

        // Datos para la gráfica de los últimos 6 meses 
        var hoy    = DateTime.UtcNow;
        var labels = Enumerable.Range(0, 6)
                               .Select(i => hoy.AddMonths(-5 + i).ToString("MMM"))
                               .ToArray();

        var dataTrans = labels.Select((lbl,i) => {
            var mes = hoy.AddMonths(-5 + i);
            return history.Count(h => h.CreatedAt.Month == mes.Month && h.CreatedAt.Year == mes.Year);
        }).ToArray();

        var dataPuntos = labels.Select((lbl,i) => {
            var mes = hoy.AddMonths(-5 + i);
            return history.Where(h => h.CreatedAt.Month == mes.Month && h.CreatedAt.Year == mes.Year)
                          .Sum(h => h.PointsSpent);
        }).ToArray();

        ViewBag.ChartLabels       = JsonConvert.SerializeObject(labels);
        ViewBag.TransaccionesData = JsonConvert.SerializeObject(dataTrans);
        ViewBag.PuntosData        = JsonConvert.SerializeObject(dataPuntos);

        return View();
    }
}
