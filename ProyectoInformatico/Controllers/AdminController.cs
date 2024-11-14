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
        private readonly PacienteService _pacienteService;
        private readonly CitaService _citaService;
        private readonly DiagnosticoService _diagnosticoService;

        public AdminController(AdminService adminService, EspecialistaService especialistaService, PacienteService pacienteService, CitaService citaService, DiagnosticoService diagnosticoService)
        {
            _adminService = adminService;
            _especialistaService = especialistaService;
            _pacienteService = pacienteService;
            _citaService = citaService;
            _diagnosticoService = diagnosticoService;
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

        [Authorize(Roles = "Admin")]
        [HttpPost("importar-csv")]
        public async Task<IActionResult> ImportarCsv(IFormFile excelFile, [FromForm] string collectionSelect)
        {
            if (excelFile == null || excelFile.Length == 0)
            {
                return BadRequest(new { mensaje = "No se ha seleccionado ningún archivo para importar." });
            }

            if (string.IsNullOrWhiteSpace(collectionSelect))
            {
                return BadRequest(new { mensaje = "Debe seleccionar una colección para importar los datos." });
            }

            try
            {
                using var stream = new StreamReader(excelFile.OpenReadStream());
                using var csvReader = new CsvHelper.CsvReader(stream, System.Globalization.CultureInfo.InvariantCulture);
                var records = csvReader.GetRecords<dynamic>().ToList();

                switch (collectionSelect.ToLower())
                {
                    case "especialista":
                        var especialistas = records.Select(r => new Especialista
                        {
                            Identificacion = int.Parse(r.Identificacion),
                            Cedula = r.Cedula,
                            Nombre = r.Nombre,
                            Especialidad = r.Especialidad,
                            Telefono = r.Telefono,
                            Direccion = r.Direccion,
                            Departamento = r.Departamento,
                            Ciudad = r.Ciudad,
                            FechaNacimiento = DateTime.Parse(r.FechaNacimiento),
                            Contraseña = BCrypt.Net.BCrypt.HashPassword(r.Contraseña),
                            Genero = r.Genero
                        }).ToList();

                        foreach (var especialista in especialistas)
                        {
                            await _especialistaService.CreateEspecialista(especialista);
                        }
                        break;

                    case "pacientes":
                        var pacientes = records.Select(r => new Paciente
                        {
                            Cedula = r.Cedula,
                            Nombre = r.Nombre,
                            Nacionalidad = r.Nacionalidad,
                            Correo = r.Correo,
                            Telefono = r.Telefono,
                            Direccion = r.Direccion,
                            Departamento = r.Departamento,
                            Ciudad = r.Ciudad,
                            FechaNacimiento = DateTime.Parse(r.FechaNacimiento),
                            TipoSangre = r.TipoSangre,
                            SemanasEmbarazo = int.Parse(r.SemanasEmbarazo),
                            FechaUltimaEcografia = DateTime.Parse(r.FechaUltimaEcografia),
                            Contraseña = BCrypt.Net.BCrypt.HashPassword(r.Contraseña),
                            Genero = r.Genero,
                            EstadoCivil = r.EstadoCivil,
                            Alergias = r.Alergias
                        }).ToList();

                        foreach (var paciente in pacientes)
                        {
                            await _pacienteService.CreatePaciente(paciente);
                        }
                        break;

                    case "citas":
                        var citas = records.Select(r => new Cita
                        {
                            Id = r.Id,
                            FechaCreacion = DateTime.Parse(r.FechaCreacion),
                            FechaCita = DateTime.Parse(r.FechaCita),
                            Estado = r.Estado,
                            IdPaciente = r.IdPaciente,
                            IdEspecialista = int.Parse(r.IdEspecialista)
                        }).ToList();

                        foreach (var cita in citas)
                        {
                            await _citaService.CreateCita(cita);
                        }
                        break;

                    case "diagnosticos":
                        var diagnosticos = records.Select(r => new Diagnostico
                        {
                            Id = r.Id,
                            Descripcion = r.Descripcion,
                            Resultados = r.Resultados,
                            Observaciones = r.Observaciones,
                            Conclusion = r.Conclusion,
                            FechaCreacion = DateTime.Parse(r.FechaCreacion),
                            FechaModificacion = DateTime.Parse(r.FechaModificacion),
                            IdPaciente = r.IdPaciente,
                            IdEspecialista = int.Parse(r.IdEspecialista),
                            IdCita = r.IdCita
                        }).ToList();

                        foreach (var diagnostico in diagnosticos)
                        {
                            await _diagnosticoService.CreateDiagnostico(diagnostico);
                        }
                        break;

                    default:
                        return BadRequest(new { mensaje = "La colección seleccionada no es válida." });
                }

                return Ok(new { mensaje = "Datos importados exitosamente." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, new { mensaje = "Error en el servidor al intentar importar los datos." });
            }
        }
    }
}