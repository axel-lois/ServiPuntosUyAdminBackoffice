using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ServiPuntosUyAdmin.Controllers
{
    public class AccountController : Controller
    {
        [HttpGet]
        public IActionResult Login()
        {
            // Siempre limpiá la sesión al mostrar el login
            HttpContext.Session.Remove("AdminLogged");
            HttpContext.Session.Remove("jwt_token");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            using (var http = new HttpClient())
            {
                http.BaseAddress = new Uri("http://localhost:5162/");
                var payload = new { email = email, password = password };
                var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");

                var response = await http.PostAsync("/api/Auth/login", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    dynamic res = JsonConvert.DeserializeObject(responseBody);
                    string token = res.data.token;

                    // Ahora obtenemos el tipo de usuario usando el token
                    var meRequest = new HttpRequestMessage(HttpMethod.Get, "/api/Auth/me");
                    meRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                    var meResponse = await http.SendAsync(meRequest);

                    string meBody = "";

                    if (meResponse.IsSuccessStatusCode)
                    {
                        meBody = await meResponse.Content.ReadAsStringAsync();
                        dynamic meRes = JsonConvert.DeserializeObject(meBody);

                        int userType = (int)meRes.data.userType;

                        if (userType == 1 || userType == 2)
                        {
                            HttpContext.Session.SetString("AdminLogged", "true");
                            HttpContext.Session.SetString("jwt_token", token);

                            string adminName = meRes.data.name; // Ajustá si tu campo es diferente
                            HttpContext.Session.SetString("AdminName", adminName);

                            string adminBranch = meRes.data.name; // Ajustá si tu campo es diferente
                            HttpContext.Session.SetString("AdminBranch", adminBranch);

                            return RedirectToAction("Index", "Home");
                        }
                        else
                        {
                            ViewBag.Error = "Solo los administradores pueden ingresar.";
                            return View();
                        }
                    }
                    else
                    {
                        meBody = await meResponse.Content.ReadAsStringAsync();
                        System.Diagnostics.Debug.WriteLine("ME RESPONSE: " + meBody); 
                        ViewBag.DebugMeBody = meBody; 
                        ViewBag.Error = "No se pudo verificar el tipo de usuario.";
                        return View();
                    }
                }
                else
                {
                    ViewBag.Error = "Usuario o contraseña incorrectos.";
                    return View();
                }
            }
        }


        public IActionResult Logout()
        {
            HttpContext.Session.Remove("AdminLogged");
            HttpContext.Session.Remove("jwt_token");
            HttpContext.Session.Remove("AdminName");
            return RedirectToAction("Login");
        }
    }
}
