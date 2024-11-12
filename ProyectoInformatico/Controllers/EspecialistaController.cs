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
    public class EspecialistaController : Controller
    {
        private readonly EspecialistaService _especialistaService;

        public EspecialistaController(EspecialistaService especialistaService)
        {
            _especialistaService = especialistaService;
        }

        [HttpGet("acceso-doctor")]
        public IActionResult AccesoDoctor()
        {
            return View("acceso-doctor");
        }

        [HttpPost("acceso-doctor")]
        public async Task<IActionResult> LoginDoctor([FromForm] string usuario, [FromForm] string password)
        {
            try
            {
                var especialista = await _especialistaService.GetEspecialistaByCedula(usuario);
                if (especialista == null)
                {
                    return BadRequest(new { mensaje = "Usuario no encontrado." });
                }

                if (!BCrypt.Net.BCrypt.Verify(password, especialista.Contraseña))
                {
                    return BadRequest(new { mensaje = "Contraseña incorrecta." });
                }

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, especialista.Nombre),
                    new Claim(ClaimTypes.NameIdentifier, especialista.Cedula),
                    new Claim(ClaimTypes.Role, "Doctor")
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties { IsPersistent = true };

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                return RedirectToAction("PanelDoctor", new { usuario = especialista.Cedula });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, new { mensaje = "Error en el servidor. Inténtelo nuevamente." });
            }
        }

        [HttpGet("registro-doctor")]
        public IActionResult RegistroDoctor()
        {
            return View("registro-doctor");
        }

        [HttpPost("registro-doctor")]
        public async Task<IActionResult> RegistrarEspecialista([FromForm] Especialista especialista)
        {
            try
            {
                var existente = await _especialistaService.GetEspecialistaByCedula(especialista.Cedula);
                if (existente != null)
                {
                    return BadRequest(new { mensaje = "Esta persona ya está registrada." });
                }

                await _especialistaService.CreateEspecialista(especialista);
                return Ok(new { mensaje = "Especialista registrado con éxito." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, new { mensaje = "Error en el servidor. Inténtelo nuevamente." });
            }
        }

        [Authorize(Roles = "Doctor")]
        [HttpGet("panel-doctor")]
        public async Task<IActionResult> PanelDoctor(string id)
        {
            try
            {
                var especialista = await _especialistaService.GetEspecialistaByCedula(id);

                if (especialista == null)
                {
                    return RedirectToAction("AccesoDoctor");
                }

                ViewBag.Nombre = especialista.Nombre;
                ViewBag.PacientesTotales = 150;
                ViewBag.CitasHoy = 8;
                ViewBag.EcografiasRealizadas = 523;

                return View("panel-doctor");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, new { mensaje = "Error en el servidor." });
            }
        }
    }
}