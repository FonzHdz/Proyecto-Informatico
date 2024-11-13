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
        private readonly EspecialistaService _especialistaService;

        public AdminController(AdminService adminService, EspecialistaService especialistaService)
        {
            _adminService = adminService;
            _especialistaService = especialistaService;
        }

        [HttpGet("admin-login")]
        public IActionResult AccesoAdmin()
        {
            return View("admin-login");
        }

        [HttpPost("admin-login")]
        public async Task<IActionResult> LoginAdmin([FromForm] string usuario, [FromForm] string contraseña)
        {
            Console.WriteLine($"Admin: {usuario}");
            Console.WriteLine($"Contraseña: {contraseña}");
            try
            {
                var admin = await _adminService.GetAdminByUsuario(usuario);
                if (admin == null)
                {
                    return BadRequest(new { mensaje = "Usuario no encontrado." });
                }

                if (!BCrypt.Net.BCrypt.Verify(contraseña, admin.Contraseña))
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

                return Ok(new { usuario = admin.Usuario });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, new { mensaje = "Error en el servidor. Inténtelo nuevamente." });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("panel-admin")]
        public async Task<IActionResult> PanelAdmin(string usuario)
        {

            Console.WriteLine($"Admin: {usuario}");
            try
            {
                var admin = await _adminService.GetAdminByUsuario(usuario);

                if (admin == null)
                {
                    Console.WriteLine("No se encuentra al administrador");
                }

                ViewBag.Nombre = admin.Usuario;
                ViewBag.PacientesTotales = 150;
                ViewBag.CitasHoy = 8;
                ViewBag.EcografiasRealizadas = 523;

                return View("admin-dashboard");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, new { mensaje = "Error en el servidor." });
            }
        }

        [Authorize(Roles = "Admin")]
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

        [Authorize(Roles = "Admin")]
        [HttpGet("editar-doctor")]
        public IActionResult EdicionDoctor()
        {
            return View("editar-doctor");
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("editar-doctor")]
        public async Task<IActionResult> EditarDoctor([FromForm] Especialista especialista)
        {
            try
            {
                var existente = await _especialistaService.GetEspecialistaByCedula(especialista.Cedula);

                if (existente == null)
                {
                    return BadRequest(new { mensaje = "El doctor no existe. No se puede editar." });
                }

                if (!string.IsNullOrWhiteSpace(especialista.Nombre))
                {
                    existente.Nombre = especialista.Nombre;
                }

                if (!string.IsNullOrWhiteSpace(especialista.Direccion))
                {
                    existente.Direccion = especialista.Direccion;
                }

                if (!string.IsNullOrWhiteSpace(especialista.Telefono))
                {
                    existente.Telefono = especialista.Telefono;
                }

                if (!string.IsNullOrWhiteSpace(especialista.Especialidad))
                {
                    existente.Especialidad = especialista.Especialidad;
                }

                var actualizado = await _especialistaService.UpdateEspecialista(existente.Identificacion, existente);

                if (!actualizado)
                {
                    return StatusCode(500, new { mensaje = "Error al guardar los cambios. Inténtalo de nuevo." });
                }

                return Ok(new { mensaje = "Doctor actualizado exitosamente." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, new { mensaje = "Error en el servidor al intentar editar el doctor." });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("eliminar-doctor")]
        public IActionResult DeleteDoctor()
        {
            return View("doctor-eliminar");
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("eliminar-doctor")]
        public async Task<IActionResult> EliminarDoctor([FromForm] Especialista especialista)
        {
            try
            {
                var existente = await _especialistaService.GetEspecialistaByIdentificacion(especialista.Identificacion);

                if (existente == null)
                {
                    return BadRequest(new { mensaje = "El doctor no existe. No se puede eliminar." });
                }

                await _especialistaService.DeleteEspecialista(especialista.Identificacion);

                return Ok(new { mensaje = "Doctor eliminado." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, new { mensaje = "Error en el servidor al intentar eliminar el doctor." });
            }
        }
    }
}