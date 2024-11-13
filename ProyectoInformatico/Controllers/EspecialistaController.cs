using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoInformatico.Models;
using ProyectoInformatico.Services;
using ProyectoInformatico.DTOs;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ProyectoInformatico.Controllers
{
    public class EspecialistaController : Controller
    {
        private readonly EspecialistaService _especialistaService;
        private readonly CitaService _citaService;
        private readonly PacienteService _pacienteService;

        public EspecialistaController(EspecialistaService especialistaService, CitaService citaService, PacienteService pacienteService)
        {
            _especialistaService = especialistaService;
            _citaService = citaService;
            _pacienteService = pacienteService;
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
                Console.WriteLine($"Usuario: {usuario}");
                Console.WriteLine($"Contraseña: {password}");
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

                return Ok(new { mensaje = "Inicio de sesión exitoso.", id = especialista.Cedula });
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
                Console.WriteLine($"Usuario: {id}");
                var especialista = await _especialistaService.GetEspecialistaByCedula(id);
                if (especialista == null)
                {
                    return NotFound("Especialista no encontrado.");
                }

                var citas = await _citaService.GetCitasByEspecialistaId(especialista.Identificacion);
                var pacientes = await _pacienteService.GetPacientesByEspecialistaId(especialista.Identificacion, _citaService);

                var informacionCitas = new List<InformacionCita>();
                foreach (var cita in citas)
                {
                    var paciente = await _pacienteService.GetPacienteByCedula(cita.IdPaciente);

                    informacionCitas.Add(new InformacionCita
                    {
                        Id = cita.Id,
                        FechaCita = cita.FechaCita,
                        Estado = char.ToUpper(cita.Estado[0]) + cita.Estado.Substring(1).ToLower(),
                        Paciente = paciente.Nombre ?? "No asignado",
                    });
                }

                ViewBag.Nombre = especialista.Nombre;
                ViewBag.PacientesTotales = pacientes.Count;
                ViewBag.CitasHoy = citas.Count(c => c.FechaCita.Date == DateTime.Today);
                ViewBag.EcografiasRealizadas = citas.Count(c => c.Estado == "realizada");
                ViewBag.Pacientes = pacientes;
                ViewBag.Citas = informacionCitas;

                return View("panel-doctor");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, "Error al cargar el panel del doctor.");
            }
        }

        [Authorize(Roles = "Doctor")]
        [HttpPost("citas/{id}/cancelar")]
        public async Task<IActionResult> CancelarCita(string id)
        {
            var cita = await _citaService.GetCitaById(id);
            if (cita == null)
            {
                return NotFound(new { mensaje = "Cita no encontrada." });
            }

            cita.Estado = "cancelada";
            var actualizado = await _citaService.UpdateCita(id, cita);

            if (!actualizado)
            {
                return StatusCode(500, new { mensaje = "No se pudo cancelar la cita." });
            }

            return Ok(new { mensaje = "Cita cancelada exitosamente." });
        }
    }
}