using Microsoft.AspNetCore.Mvc;

namespace ServiPuntosUyAdmin.Controllers
{
    public class AccountController : Controller
    {
        // GET: /Account/Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
         public IActionResult Login(string email, string password)
        {
            // Simulación de validación simple solo para prueba, TODO: cambiar
            if (email == "admin@servipuntos.uy" && password == "admin123")
            {
                // TODO: Implementar autenticación real y redirección al dashboard
                return RedirectToAction("Index", "Home");
            }
            ViewBag.Error = "Usuario o contraseña incorrectos.";
            return View();
        }
    }
}