using Microsoft.AspNetCore.Mvc;

namespace ServiPuntosUyAdmin.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            ViewData["Title"] = "Administradores";
            return View();
        }
    }
}