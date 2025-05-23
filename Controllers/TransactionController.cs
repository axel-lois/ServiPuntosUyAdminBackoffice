using Microsoft.AspNetCore.Mvc;

namespace ServiPuntosUyAdmin.Controllers
{
    public class TransactionController : Controller
    {
        public IActionResult Index()
        {
            ViewData["Title"] = "Transacciones";
            return View();
        }
    }
}