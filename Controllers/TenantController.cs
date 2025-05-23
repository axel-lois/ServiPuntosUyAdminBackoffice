using Microsoft.AspNetCore.Mvc;

namespace ServiPuntosUyAdmin.Controllers
{
    public class TenantController : Controller
    {
        public IActionResult Index()
        {
            ViewData["Title"] = "Tenant";
            return View();
        }
    }
}