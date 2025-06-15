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
        public async Task<IActionResult> Login(string email, string password, string userType, string tenantName = null)
        {
            using (var http = new HttpClient())
            {
                http.BaseAddress = new Uri("http://localhost:5162/");
                var payload = new { email = email, password = password };
                var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");

                var request = new HttpRequestMessage(HttpMethod.Post, "/api/Auth/login") { Content = content };
                request.Headers.Add("X-User-Type", userType);

                if ((userType == "Tenant" || userType == "Branch") && !string.IsNullOrEmpty(tenantName))
                {
                    request.Headers.Add("X-Tenant-Name", tenantName);
                }

                var response = await http.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    dynamic res = JsonConvert.DeserializeObject(responseBody);
                    string token = res.data.token;

                    // Obtener el tipo real de usuario con el token (opcional, según tu backend)
                    var meRequest = new HttpRequestMessage(HttpMethod.Get, "/api/Auth/me");
                    meRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                    var meResponse = await http.SendAsync(meRequest);

                    string meBody = "";

                    if (meResponse.IsSuccessStatusCode)
                    {
                        meBody = await meResponse.Content.ReadAsStringAsync();
                        dynamic meRes = JsonConvert.DeserializeObject(meBody);

                        int userTypeReal = (int)meRes.data.userType;

                        HttpContext.Session.SetString("AdminLogged", "true");
                        HttpContext.Session.SetString("jwt_token", token);
                        HttpContext.Session.SetString("user_type", userTypeReal == 1 ? "admin_central" : userTypeReal == 2 ? "admin_tenant" : "admin_branch");

                        if (userTypeReal == 2 && meRes.data.tenantId != null)
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

        [HttpPost]
        public async Task<IActionResult> Signup(string email, string password, string name, string userType)
        {
            using (var http = new HttpClient())
            {
                http.BaseAddress = new Uri("http://localhost:5162/");
                var payload = new { email = email, password = password, name = name };
                var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json"); 

                var request = new HttpRequestMessage(HttpMethod.Post, "/api/Auth/signup") { Content = content };
                request.Headers.Add("X-User-Type", userType);

                var response = await http.SendAsync(request);

                var responseBody = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "¡Usuario registrado correctamente! Ahora podés iniciar sesión.";
                    return RedirectToAction("Login");
                }
                else
                {
                    ViewBag.Error = "No se pudo registrar el usuario. " + responseBody;
                    return View("Login");
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
