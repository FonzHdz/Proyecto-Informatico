using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoInformatico.Models;
using ProyectoInformatico.Services;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ProyectoInformatico.Controllers
{
    public class AdminController : Controller
    {
        private readonly AdminService _adminService;

        public AdminController(AdminService adminService)
        {
            _adminService = adminService;
        }

        [HttpGet("admin-login")]
        public IActionResult AccesoAdmin()
        {
            return View("admin-login");
        }

        [HttpPost("admin-login")]
        public async Task<IActionResult> LoginAdmin([FromForm] string usuario, [FromForm] string password)
        {
            try
            {
                var admin = await _adminService.GetAdminByUsuario(usuario);
                if (admin == null)
                {
                    return BadRequest(new { mensaje = "Usuario no encontrado." });
                }

                if (!BCrypt.Net.BCrypt.Verify(password, admin.Contraseña))
                {
                    return BadRequest(new { mensaje = "Contraseña incorrecta." });
                }

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, admin.Usuario),
                    new Claim(ClaimTypes.NameIdentifier, admin.Id),
                    new Claim(ClaimTypes.Role, "Admin")
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties { IsPersistent = true };

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                return RedirectToAction("PanelAdmin", new { usuario = admin.Usuario });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, new { mensaje = "Error en el servidor. Inténtelo nuevamente." });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("panel-admin")]
        public async Task<IActionResult> PanelAdmin(string id)
        {
            try
            {
                var admin = await _adminService.GetAdminByUsuario(id);

                if (admin == null)
                {
                    return RedirectToAction("AccesoAdmin");
                }

                ViewBag.Nombre = admin.Usuario;
                ViewBag.PacientesTotales = 150;
                ViewBag.CitasHoy = 8;
                ViewBag.EcografiasRealizadas = 523;

                return View("panel-admin");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, new { mensaje = "Error en el servidor." });
            }
        }
    }
}