using Microsoft.AspNetCore.Mvc;

namespace ServiPuntosUyAdmin.Controllers
{
    public class StationController : Controller
    {
        public IActionResult Index()
        {
            ViewData["Title"] = "Estaciones";
            return View();
        }
    }
}