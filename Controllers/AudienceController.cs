using Microsoft.AspNetCore.Mvc;

namespace ServiPuntosUyAdmin.Controllers
{
    public class AudienceController : Controller
    {
        public IActionResult Index()
        {
            ViewData["Title"] = "Audiencias";
            return View();
        }
    }
}