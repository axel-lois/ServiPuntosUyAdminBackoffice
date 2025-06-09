using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ServiPuntosUyAdmin.Models;

public class HomeController : Controller
{
    public async Task<IActionResult> Index()
    {
        int usuarios = 0;
        int transacciones = 0;
        int ofertasActivas = 0;
        int puntosCanjeados = 0; // Si lo tenés en el endpoint, si no, dejar 0 o "----"

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
                if (result != null && !result.Error && result.Data != null)
                {
                    usuarios = result.Data.Users?.Total ?? 0;
                    transacciones = result.Data.Transactions?.Total ?? 0;
                    ofertasActivas = result.Data.Promotions?.Total ?? 0;
                }
            }
        }

        ViewBag.Usuarios = usuarios;
        ViewBag.Transacciones = transacciones;
        ViewBag.OfertasActivas = ofertasActivas;
        return View();
    }


}
