using Microsoft.AspNetCore.Mvc;

namespace ServiPuntosUyAdmin.Controllers
{
    public class OfferController : Controller
    {
        public IActionResult Index()
        {
            ViewData["Title"] = "Ofertas";
            return View();
        }
    }
}