using Microsoft.AspNetCore.Mvc;
using ProyectoInformatico.Models;
using ProyectoInformatico.DTOs;
using ProyectoInformatico.Services;
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
using System;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Drawing;
using FellowOakDicom;

namespace ProyectoInformatico.Controllers
{
    public class PacienteController : Controller
    {
        private readonly PacienteService _pacienteService;
        private readonly EspecialistaService _especialistaService;
        private readonly CitaService _citaService;
        private readonly DiagnosticoService _diagnosticoService;
        private readonly VideoEcografiaService _videoEcografiaService;
        private readonly ImagenRadiologicaService _imagenRadiologicaService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly EmailSettings _emailSettings;
        private readonly string _apiKey;

        public PacienteController(CitaService citaService, DiagnosticoService diagnosticoService, EspecialistaService especialistaService, PacienteService pacienteService, VideoEcografiaService videoEcografiaService, ImagenRadiologicaService imagenRadiologicaService, IWebHostEnvironment webHostEnvironment, EmailSettings emailSettings, IConfiguration configuration)
        {
            _citaService = citaService;
            _diagnosticoService = diagnosticoService;
            _especialistaService = especialistaService;
            _pacienteService = pacienteService;
            _videoEcografiaService = videoEcografiaService;
            _imagenRadiologicaService = imagenRadiologicaService;
            _webHostEnvironment = webHostEnvironment;
            _emailSettings = emailSettings;
            _apiKey = configuration["ZamzarApiKey"];
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
                        Id = cita.Id,
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
                ViewBag.ProximaCita = citas
                    .Where(c => c.FechaCita > DateTime.Now && c.Estado.ToLower() == "pendiente")
                    .OrderBy(c => c.FechaCita)
                    .FirstOrDefault()?.FechaCita.ToString("dd/MM/yyyy hh:mm tt") ?? "Sin citas programadas";
                ViewBag.UltimaEcografia = citas
                    .Where(c => c.Estado.ToLower() == "realizada")
                    .OrderByDescending(c => c.FechaCita)
                    .FirstOrDefault()?.FechaCita.ToString("dd/MM/yyyy") ?? "Sin ecografías";
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

        [Authorize(Roles = "Paciente")]
        [HttpPost("paciente/enviar-diagnostico")]
        public async Task<IActionResult> EnviarDiagnostico([FromBody] PdfDataRequest request)
        {
            string pdfTemplatePath = Path.Combine(_webHostEnvironment.WebRootPath, "public", "DiagnosticoPlantilla.pdf");
            string rutaTemporalDirectorio = Path.Combine(Directory.GetCurrentDirectory(), "ArchivosTemporales");
            Directory.CreateDirectory(rutaTemporalDirectorio);

            using var outputStream = new MemoryStream();
            Console.WriteLine($"Recibido DiagnosticoId: {request.DiagnosticoId}");

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
                if (especialista == null)
                {
                    return BadRequest(new { mensaje = "Especialista no encontrado." });
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
                    formFields.SetField("var-tel_paciente", $"+57 {paciente.Telefono}");
                    formFields.SetField("var-date_cita", diagnostico.FechaCreacion.ToString("dd - MM - yyyy"));
                    formFields.SetField("var-hora_cita", diagnostico.FechaCreacion.ToString("hh:mm tt"));
                    formFields.SetField("var-servicio", "Evaluación morfológica fetal");
                    formFields.SetField("var-meses_embarazo", paciente.SemanasEmbarazo.ToString() + " semanas");
                    formFields.SetField("var-descripcion", diagnostico.Descripcion ?? "N/A");
                    formFields.SetField("var-resultados", diagnostico.Resultados ?? "N/A");
                    formFields.SetField("var-observaciones", diagnostico.Observaciones ?? "N/A");
                    formFields.SetField("var-conclusiones", diagnostico.Conclusion ?? "N/A");
                    formFields.SetField("var-nombre_especialista", "Dr. " + especialista.Nombre);
                    formFields.SetField("var-especialidad", "Especialista en " + especialista.Especialidad);
                    stamper.FormFlattening = true;
                }

                byte[] pdfBytes = outputStream.ToArray();

                var videoEcografia = await _videoEcografiaService.GetByDiagnosticoId(diagnostico.Id);
                string rutaTemporalMp4 = null;

                if (videoEcografia != null && !string.IsNullOrEmpty(videoEcografia.UrlDescarga))
                {
                    var rutaTemporalAvi = Path.Combine(rutaTemporalDirectorio, $"Ecografia_{diagnostico.Id}.avi");
                    using (var clientHttp = new HttpClient())
                    {
                        var response = await clientHttp.GetAsync(videoEcografia.UrlDescarga);
                        response.EnsureSuccessStatusCode();
                        await using var fileStream = new FileStream(rutaTemporalAvi, FileMode.Create, FileAccess.Write);
                        await response.Content.CopyToAsync(fileStream);
                    }

                    rutaTemporalMp4 = await ConvertVideoToMp4(rutaTemporalAvi, diagnostico.Id);
                }

                var imagenesRadiologicas = await _imagenRadiologicaService.GeImagenesDiagnosticoId(diagnostico.Id);
                List<string> imagenesPng = new List<string>();

                foreach (var imagen in imagenesRadiologicas)
                {
                    string rutaTemporalPng = Path.Combine(rutaTemporalDirectorio, $"Radiologica_{imagen.Id}.png");

                    using (var clientHttp = new HttpClient())
                    {
                        var response = await clientHttp.GetAsync(imagen.UrlDescarga);
                        response.EnsureSuccessStatusCode();
                        await using var fileStream = new FileStream(rutaTemporalPng, FileMode.Create, FileAccess.Write);
                        await response.Content.CopyToAsync(fileStream);
                    }

                    imagenesPng.Add(rutaTemporalPng);
                }

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Sistema de Diagnósticos", _emailSettings.SenderEmail));
                message.To.Add(new MailboxAddress(paciente.Nombre, paciente.Correo));
                message.Subject = "Diagnóstico Médico";

                var builder = new BodyBuilder
                {
                    TextBody = $"Estimado(a) {paciente.Nombre},\n\nAdjuntamos su diagnóstico del día {diagnostico.FechaCreacion:dd/MM/yyyy} junto con los recursos multimedia asociados."
                };

                builder.Attachments.Add($"Diagnostico_{paciente.Nombre}.pdf", pdfBytes, new ContentType("application", "pdf"));

                if (!string.IsNullOrEmpty(rutaTemporalMp4) && System.IO.File.Exists(rutaTemporalMp4))
                {
                    builder.Attachments.Add($"Ecografia_{diagnostico.Id}.mp4", System.IO.File.ReadAllBytes(rutaTemporalMp4), new ContentType("video", "mp4"));
                }

                foreach (var rutaPng in imagenesPng)
                {
                    if (System.IO.File.Exists(rutaPng))
                    {
                        builder.Attachments.Add(Path.GetFileName(rutaPng), System.IO.File.ReadAllBytes(rutaPng), new ContentType("image", "png"));
                    }
                }

                message.Body = builder.ToMessageBody();

                using var client = new SmtpClient();
                client.Connect(_emailSettings.SmtpServer, _emailSettings.SmtpPort, false);
                client.Authenticate(_emailSettings.SenderEmail, _emailSettings.SenderPassword);
                await client.SendAsync(message);
                client.Disconnect(true);

                foreach (var rutaPng in imagenesPng)
                {
                    if (System.IO.File.Exists(rutaPng))
                    {
                        System.IO.File.Delete(rutaPng);
                    }
                }

                if (!string.IsNullOrEmpty(rutaTemporalMp4) && System.IO.File.Exists(rutaTemporalMp4))
                {
                    System.IO.File.Delete(rutaTemporalMp4);
                }

                return Ok(new { mensaje = "El diagnóstico y recursos multimedia se han enviado exitosamente." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al enviar el diagnóstico: {ex.Message}");
                return StatusCode(500, new { mensaje = "Error al enviar el diagnóstico. Por favor, intente nuevamente." });
            }
        }

        private async Task<string> ConvertVideoToMp4(string rutaTemporalAvi, string diagnosticoId)
        {
            var httpClient = new HttpClient();

            var byteArray = System.Text.Encoding.ASCII.GetBytes($"{_apiKey}:");
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

            var content = new MultipartFormDataContent();
            content.Add(new ByteArrayContent(System.IO.File.ReadAllBytes(rutaTemporalAvi)), "source_file", $"Ecografia_{diagnosticoId}.avi");
            content.Add(new StringContent("mp4"), "target_format");

            var response = await httpClient.PostAsync("https://api.zamzar.com/v1/jobs", content);
            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var jsonDocument = JsonDocument.Parse(jsonResponse);
            var jobId = jsonDocument.RootElement.GetProperty("id").GetInt32();

            while (true)
            {
                var jobResponse = await httpClient.GetAsync($"https://api.zamzar.com/v1/jobs/{jobId}");
                jobResponse.EnsureSuccessStatusCode();

                var jobJson = await jobResponse.Content.ReadAsStringAsync();
                var jobStatus = JsonDocument.Parse(jobJson).RootElement.GetProperty("status").GetString();

                if (jobStatus == "successful")
                {
                    var targetFileId = JsonDocument.Parse(jobJson).RootElement.GetProperty("target_files")[0].GetProperty("id").GetInt32();
                    var downloadUrl = $"https://api.zamzar.com/v1/files/{targetFileId}/content";

                    var convertedResponse = await httpClient.GetAsync(downloadUrl);
                    var rutaMp4 = Path.Combine(Path.GetDirectoryName(rutaTemporalAvi), $"Ecografia_{diagnosticoId}.mp4");

                    await using var fs = new FileStream(rutaMp4, FileMode.Create, FileAccess.Write);
                    await convertedResponse.Content.CopyToAsync(fs);

                    return rutaMp4;
                }
                else if (jobStatus == "failed")
                {
                    throw new Exception("La conversión falló en Zamzar.");
                }

                await Task.Delay(5000);
            }
        }

        [Authorize(Roles = "Paciente")]
        [HttpGet("solicitar-cita")]
        public async Task<IActionResult> SolicitarCita()
        {
            try
            {
                var especialistas = await _especialistaService.GetAllEspecialistas();
                ViewBag.Especialistas = especialistas;
                return View("solicitar-cita");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener especialistas: {ex.Message}");
                return StatusCode(500, new { mensaje = "Error al cargar los especialistas. Intente nuevamente." });
            }
        }

        [Authorize(Roles = "Paciente")]
        [HttpPost("citas/crear-cita")]
        public async Task<IActionResult> CrearCita([FromBody] CitaRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.FechaCita) || string.IsNullOrWhiteSpace(request.Especialista.ToString()))
                {
                    return BadRequest(new { mensaje = "Todos los campos son obligatorios." });
                }

                var pacienteCedula = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrWhiteSpace(pacienteCedula))
                {
                    return Unauthorized(new { mensaje = "No se pudo identificar al paciente." });
                }

                var especialista = await _especialistaService.GetEspecialistaByIdentificacion(request.Especialista);

                if (especialista == null)
                {
                    return BadRequest(new { mensaje = "El especialista seleccionado no existe." });
                }

                var nuevaCita = new Cita
                {
                    FechaCreacion = DateTime.UtcNow,
                    FechaCita = DateTime.Parse(request.FechaCita),
                    Estado = "pendiente",
                    IdPaciente = pacienteCedula,
                    IdEspecialista = especialista.Identificacion,
                };

                await _citaService.CreateCita(nuevaCita);

                return Ok(new { mensaje = "Cita creada exitosamente." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al crear la cita: {ex.Message}");
                return StatusCode(500, new { mensaje = "Error en el servidor. Por favor, intente nuevamente." });
            }
        }
    }
}