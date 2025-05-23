using Microsoft.AspNetCore.Mvc;

namespace ServiPuntosUyAdmin.Controllers
{
    public class NotificationController : Controller
    {
        public IActionResult Index()
        {
            ViewData["Title"] = "Notificaciones";
            return View();
        }
    }
}