using Microsoft.AspNetCore.Mvc;

namespace ServiPuntosUyAdmin.Controllers
{
    public class CustomizationController : Controller
    {
        public IActionResult Index()
        {
            ViewData["Title"] = "Personalización";
            return View();
        }
    }
}