using Microsoft.AspNetCore.Mvc;

namespace ServiPuntosUyAdmin.Controllers
{
    public class ParameterController : Controller
    {
        public IActionResult Index()
        {
            ViewData["Title"] = "Parametros Generales";
            return View();
        }
    }
}