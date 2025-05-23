using Microsoft.AspNetCore.Mvc;

namespace ServiPuntosUyAdmin.Controllers
{
    public class ProductController : Controller
    {
        public IActionResult Index()
        {
            ViewData["Title"] = "Productos";
            return View();
        }
    }
}