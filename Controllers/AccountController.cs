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
            HttpContext.Session.Remove("AdminLogged");
            HttpContext.Session.Remove("jwt_token");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password, string userType)
        {
            using (var http = new HttpClient())
            {
                http.BaseAddress = new Uri("http://localhost:5162/");
                var payload = new { email = email, password = password };
                var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");

                // PASA el tipo de usuario en el header que pide el backend:
                content.Headers.Add("X-User-Type", userType);

                var request = new HttpRequestMessage(HttpMethod.Post, "/api/Auth/login");
                request.Content = content;

                var response = await http.SendAsync(request);

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

                        int _userType = (int)meRes.data.userType;

                        if (_userType == 1 || _userType == 2 || _userType == 3)
                        {
                            HttpContext.Session.SetString("AdminLogged", "true");
                            HttpContext.Session.SetString("jwt_token", token);
                            HttpContext.Session.SetString("user_type", _userType == 1 ? "admin_central" : (_userType == 2 ? "admin_tenant" : "admin_branch"));

                            if (_userType == 2 && meRes.data.tenantId != null)
                            {
                                string tenantIdStr = meRes.data.tenantId.ToString();
                                HttpContext.Session.SetString("tenant_id", tenantIdStr);
                            }

                            string adminName = meRes.data.name;
                            HttpContext.Session.SetString("AdminName", adminName);

                            string adminBranch = meRes.data.name;
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
