using Microsoft.AspNetCore.Mvc;
using ProyectoInformatico.Models;
using ProyectoInformatico.DTOs;
using ProyectoInformatico.Services;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using iTextSharp.text.pdf;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using MailKit.Net.Smtp;
using MimeKit;

namespace ProyectoInformatico.Controllers
{
    public class PacienteController : Controller
    {
        private readonly PacienteService _pacienteService;
        private readonly EspecialistaService _especialistaService;
        private readonly CitaService _citaService;
        private readonly DiagnosticoService _diagnosticoService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly EmailSettings _emailSettings;

        public PacienteController(CitaService citaService, DiagnosticoService diagnosticoService, EspecialistaService especialistaService, PacienteService pacienteService, IWebHostEnvironment webHostEnvironment, EmailSettings emailSettings)
        {
            _citaService = citaService;
            _diagnosticoService = diagnosticoService;
            _especialistaService = especialistaService;
            _pacienteService = pacienteService;
            _webHostEnvironment = webHostEnvironment;
            _emailSettings = emailSettings;
        }

        [HttpGet("registro-paciente")]
        public IActionResult RegistroPaciente()
        {
            return View("registro-paciente");
        }

        [HttpPost("registro-paciente")]
        public async Task<IActionResult> RegistrarPaciente([FromForm] Paciente paciente)
        {
            try
            {
                var existente = await _pacienteService.GetPacienteByCedula(paciente.Cedula);
                if (existente != null)
                {
                    return BadRequest(new { mensaje = "Este paciente ya está registrado." });
                }

                await _pacienteService.CreatePaciente(paciente);
                return Ok(new { mensaje = "Paciente registrado con éxito." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, new { mensaje = "Error en el servidor. Inténtelo nuevamente." });
            }
        }

        [HttpGet("acceso-paciente")]
        public IActionResult AccesoPaciente()
        {
            return View("acceso-paciente");
        }

        [HttpPost("acceso-paciente")]
        public async Task<IActionResult> LoginPaciente([FromForm] string cedula, [FromForm] string password)
        {
            try
            {
                Console.WriteLine($"Correo usuario: {cedula}");

                var paciente = await _pacienteService.GetPacienteByCedula(cedula);

                if (paciente == null)
                {
                    return BadRequest(new { mensaje = "Paciente no encontrado." });
                }

                if (!BCrypt.Net.BCrypt.Verify(password, paciente.Contraseña))
                {
                    return BadRequest(new { mensaje = "Contraseña incorrecta." });
                }

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, paciente.Nombre),
                    new Claim(ClaimTypes.NameIdentifier, paciente.Cedula),
                    new Claim(ClaimTypes.Role, "Paciente")
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties { IsPersistent = true };

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                return Ok(new { mensaje = "Inicio de sesión exitoso.", pacienteId = paciente.Cedula });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, new { mensaje = "Error en el servidor." });
            }
        }

        [Authorize(Roles = "Paciente")]
        [HttpGet("panel-paciente")]
        public async Task<IActionResult> PanelPaciente(string cedula)
        {
            try
            {
                var paciente = await _pacienteService.GetPacienteByCedula(cedula);
                if (paciente == null)
                {
                    return RedirectToAction("AccesoPaciente");
                }

                var diagnosticos = await _diagnosticoService.GetDiagnosticosByPacienteId(paciente.Cedula);
                var citas = await _citaService.GetCitasByPacienteId(paciente.Cedula);

                var informacionCitas = new List<InformacionCita>();
                foreach (var cita in citas)
                {
                    var especialista = await _especialistaService.GetEspecialistaByIdentificacion(cita.IdEspecialista);

                    informacionCitas.Add(new InformacionCita
                    {
                        FechaCita = cita.FechaCita,
                        Estado = char.ToUpper(cita.Estado[0]) + cita.Estado.Substring(1).ToLower(),
                        Especialista = especialista?.Nombre ?? "No asignado"
                    });
                }

                Console.WriteLine($"Correo usuario: {paciente.Correo}");

                ViewBag.Citas = informacionCitas;
                ViewBag.PacienteNombre = paciente.Nombre;
                ViewBag.Correo = paciente.Correo;
                ViewBag.Cedula = paciente.Cedula;
                ViewBag.FechaNacimiento = paciente.FechaNacimiento.ToString("yyyy-MM-dd");
                ViewBag.Telefono = paciente.Telefono;
                ViewBag.ProximaCita = citas.OrderBy(c => c.FechaCita).FirstOrDefault()?.FechaCita.ToString("dd/MM/yyyy hh:mm tt") ?? "Sin citas programadas";
                ViewBag.UltimaEcografia = paciente.FechaUltimaEcografia.Year == 1 ? "Sin ecografías" : paciente.FechaUltimaEcografia.ToString("dd/MM/yyyy");
                ViewBag.SemanasEmbarazo = paciente.SemanasEmbarazo;
                ViewBag.HistorialMedico = diagnosticos;

                return View("panel-paciente");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, new { mensaje = "Error en el servidor." });
            }
        }

        [HttpPost("actualizar-perfil")]
        public async Task<IActionResult> ActualizarPerfil([FromForm] Paciente paciente)
        {
            try
            {
                Console.WriteLine($"Usuario recibido: {paciente.Cedula}");
                var pacienteExistente = await _pacienteService.GetPacienteByCedula(paciente.Cedula);
                if (pacienteExistente == null)
                {
                    return NotFound(new { mensaje = "Paciente no encontrado." });
                }

                pacienteExistente.Nombre = paciente.Nombre;
                pacienteExistente.Correo = paciente.Correo;
                pacienteExistente.Telefono = paciente.Telefono;
                pacienteExistente.FechaNacimiento = paciente.FechaNacimiento;

                var actualizado = await _pacienteService.UpdatePaciente(pacienteExistente.Cedula, pacienteExistente);
                if (!actualizado)
                {
                    return BadRequest(new { mensaje = "No se pudo actualizar el perfil." });
                }

                return Ok(new { mensaje = "Perfil actualizado correctamente." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, new { mensaje = "Error en el servidor." });
            }
        }

        [Authorize(Roles = "Paciente")]
        [HttpPost("paciente/enviar-diagnostico")]
        public async Task<IActionResult> EnviarDiagnostico([FromBody] PdfDataRequest request)
        {
            string pdfTemplatePath = Path.Combine(_webHostEnvironment.WebRootPath, "public", "DiagnosticoPlantilla.pdf");

            using var outputStream = new MemoryStream();

            try
            {
                var diagnostico = await _diagnosticoService.GetDiagnosticoById(request.DiagnosticoId);
                if (diagnostico == null)
                {
                    return BadRequest(new { mensaje = "El diagnóstico no fue encontrado." });
                }

                var paciente = await _pacienteService.GetPacienteByCedula(diagnostico.IdPaciente);
                if (paciente == null)
                {
                    return BadRequest(new { mensaje = "Paciente no encontrado." });
                }

                var especialista = await _especialistaService.GetEspecialistaByIdentificacion(diagnostico.IdEspecialista);
                if (paciente == null)
                {
                    return BadRequest(new { mensaje = "Paciente no encontrado." });
                }

                using (var pdfReader = new PdfReader(pdfTemplatePath))
                using (var stamper = new PdfStamper(pdfReader, outputStream))
                {
                    string password = paciente.Cedula;
                    stamper.SetEncryption(
                        Encoding.UTF8.GetBytes(password),
                        Encoding.UTF8.GetBytes(password),
                        PdfWriter.ALLOW_PRINTING | PdfWriter.ALLOW_COPY,
                        PdfWriter.ENCRYPTION_AES_128
                    );

                    var formFields = stamper.AcroFields;
                    formFields.SetField("var-nombre_paciente", paciente.Nombre);
                    formFields.SetField("var-cedula_paciente", paciente.Cedula);
                    formFields.SetField("var-date_n_paciente", paciente.FechaNacimiento.ToString("dd - MM - yyyy"));
                    formFields.SetField("var-edad_paciente", ((DateTime.Now - paciente.FechaNacimiento).TotalDays / 365).ToString("0") + " años");
                    formFields.SetField("var-tel_paciente", "+57 ", paciente.Telefono);
                    formFields.SetField("var-date_cita", diagnostico.FechaCreacion.ToString("dd - MM - yyyy"));
                    formFields.SetField("var-hora_cita", diagnostico.FechaCreacion.ToString("hh:mm tt"));
                    formFields.SetField("var-servicio", "Evaluación morfológica fetal");
                    formFields.SetField("var-meses_embarazo", paciente.SemanasEmbarazo.ToString() + " semanas");
                    formFields.SetField("var-descripcion", diagnostico.Descripcion ?? "N/A");
                    formFields.SetField("var-resultados", diagnostico.Resultados ?? "N/A");
                    formFields.SetField("var-observaciones", diagnostico.Observaciones ?? "N/A");
                    formFields.SetField("var-conclusiones", diagnostico.Conclusion ?? "N/A");
                    formFields.SetField("var-nombre_especialista", "Dr. " + especialista.Nombre);
                    formFields.SetField("var-especialidad", "Especialidad " + especialista.Especialidad);

                    stamper.FormFlattening = true;
                }

                byte[] pdfBytes = outputStream.ToArray();

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Sistema de Diagnósticos", _emailSettings.SenderEmail));
                message.To.Add(new MailboxAddress(paciente.Nombre, paciente.Correo));
                message.Subject = "Diagnóstico Médico";

                var builder = new BodyBuilder
                {
                    TextBody = "Estimado(a) " + paciente.Nombre + ",\n\nEstamos enviando su diagnóstico de acuerdo a su solicitud. Se adjuntan el diagnóstico del día " + diagnostico.FechaCreacion.ToString("dd/MM/yyyy") + " junto al video MP4 de la ecografía realizada y las imágenes radiológicas correspondientes.",
                };

                builder.Attachments.Add($"Diagnostico_{paciente.Nombre}.pdf", pdfBytes, new ContentType("application", "pdf"));

                message.Body = builder.ToMessageBody();

                using var client = new SmtpClient();
                client.Connect(_emailSettings.SmtpServer, _emailSettings.SmtpPort, false);
                client.Authenticate(_emailSettings.SenderEmail, _emailSettings.SenderPassword);
                await client.SendAsync(message);
                client.Disconnect(true);

                return Ok(new { mensaje = "El diagnóstico se ha enviado exitosamente al correo del paciente." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al generar el PDF: {ex.Message}");
                return StatusCode(500, new { mensaje = "Error al generar el PDF. Por favor, intente nuevamente." });
            }
        }
    }
}