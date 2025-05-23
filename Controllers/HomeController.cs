using Microsoft.AspNetCore.Mvc;

namespace ServiPuntosUyAdmin.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            ViewData["Title"] = "Dashboard";
            // Simulamos datos, después los traemos de la DB
            ViewBag.Usuarios = 1342;
            ViewBag.PuntosCanjeados = 87390;
            ViewBag.Transacciones = 25140;
            ViewBag.OfertasActivas = 12;
            return View();
        }
    }
}
