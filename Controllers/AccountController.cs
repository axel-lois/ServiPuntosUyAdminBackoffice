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

        public IActionResult Logout()
        {
            // Borra la sesión de usuario
            HttpContext.Session.Clear();
            // Opcional: también podés limpiar TempData, cookies, etc.
            return RedirectToAction("Login", "Account");
        }

        // POST: /Account/Login
        [HttpPost]
         public IActionResult Login(string email, string password)
        {
            // Simulación de validación simple solo para prueba, TODO: cambiar
            if (email == "admin@servipuntos.uy" && password == "admin123")
            {
                HttpContext.Session.SetString("AdminLogged", "true");

                return RedirectToAction("Index", "Home");
            }
            ViewBag.Error = "Usuario o contraseña incorrectos.";
            return View();
        }
    }
}