using Microsoft.AspNetCore.Mvc;
using ProyectoInformatico.Models;
using ProyectoInformatico.Services;
using System.Threading.Tasks;
using System;
using System.IO;
using Microsoft.AspNetCore.Authorization;

namespace ProyectoInformatico.Controllers
{
    public class DiagnosticoController : Controller
    {
        private readonly DiagnosticoService _diagnosticoService;
        private readonly CitaService _citaService;
        private readonly PacienteService _pacienteService;
        private readonly BlobStorageService _blobStorageService;
        private readonly VideoEcografiaService _videoEcografiaService;
        private readonly ImagenRadiologicaService _imagenRadiologicaService;

        public DiagnosticoController(DiagnosticoService diagnosticoService, CitaService citaService, PacienteService pacienteService, BlobStorageService blobStorageService, VideoEcografiaService videoEcografiaService, ImagenRadiologicaService imagenRadiologicaService)
        {
            _diagnosticoService = diagnosticoService;
            _citaService = citaService;
            _pacienteService = pacienteService;
            _blobStorageService = blobStorageService;
            _videoEcografiaService = videoEcografiaService;
            _imagenRadiologicaService = imagenRadiologicaService;
        }

        [Authorize(Roles = "Doctor")]
        [HttpGet("diagnostico/{citaId}")]
        public async Task<IActionResult> AbrirDiagnostico(string citaId)
        {
            var cita = await _citaService.GetCitaById(citaId);
            if (cita == null) return NotFound("Cita no encontrada.");

            var paciente = await _pacienteService.GetPacienteByCedula(cita.IdPaciente);
            if (paciente == null) return NotFound("Paciente no encontrado.");

            var diagnosticoExistente = await _diagnosticoService.GetDiagnosticoByCitaId(citaId);

            if (diagnosticoExistente != null)
            {
                var videoExistente = await _videoEcografiaService.GetByDiagnosticoId(diagnosticoExistente.Id);

                ViewBag.Diagnostico = diagnosticoExistente;
                ViewBag.PacienteNombre = paciente.Nombre;
                ViewBag.Descripcion = diagnosticoExistente.Descripcion;
                ViewBag.Resultados = diagnosticoExistente.Resultados;
                ViewBag.Observaciones = diagnosticoExistente.Observaciones;
                ViewBag.Conclusion = diagnosticoExistente.Conclusion;

                if (videoExistente != null && !string.IsNullOrEmpty(videoExistente.UrlDescarga))
                {
                    var rutaTemporalDirectorio = Path.Combine(Directory.GetCurrentDirectory(), "ArchivosTemporales");
                    Directory.CreateDirectory(rutaTemporalDirectorio);

                    var rutaTemporal = Path.Combine(rutaTemporalDirectorio, $"Ecografia_{diagnosticoExistente.Id}.avi");

                    Console.WriteLine($"Ruta de la carpeta temporal: {rutaTemporal}");

                    using (var client = new HttpClient())
                    {
                        var response = await client.GetAsync(videoExistente.UrlDescarga);
                        response.EnsureSuccessStatusCode();
                        await using var fileStream = new FileStream(rutaTemporal, FileMode.Create, FileAccess.Write);
                        await response.Content.CopyToAsync(fileStream);
                    }

                    ViewBag.VideoEcografia = $"/ArchivosTemporales/Ecografia_{diagnosticoExistente.Id}.avi";
                }

                return View("~/Views/Especialista/diagnostico.cshtml");
            }

            ViewBag.PacienteNombre = paciente.Nombre;
            return View("~/Views/Especialista/diagnostico.cshtml");
        }

        [Authorize(Roles = "Doctor")]
        [HttpPost("diagnostico/guardar")]
        public async Task<IActionResult> GuardarDiagnostico([FromForm] Diagnostico diagnostico, [FromForm] IFormFile videoEcografia, [FromForm] List<IFormFile> imagenesRadiologicas)
        {
            try
            {
                Console.WriteLine("Iniciando el proceso de guardar diagnóstico.");

                diagnostico.FechaModificacion = DateTime.Now;

                if (!string.IsNullOrEmpty(diagnostico.Id))
                {
                    Console.WriteLine($"Actualizando diagnóstico con ID: {diagnostico.Id}");
                    var diagnosticoExistente = await _diagnosticoService.GetDiagnosticoById(diagnostico.Id);
                    if (diagnosticoExistente == null)
                    {
                        Console.WriteLine("Diagnóstico no encontrado para actualizar.");
                        return NotFound(new { mensaje = "Diagnóstico no encontrado para actualizar." });
                    }

                    diagnosticoExistente.Descripcion = diagnostico.Descripcion;
                    diagnosticoExistente.Resultados = diagnostico.Resultados;
                    diagnosticoExistente.Observaciones = diagnostico.Observaciones;
                    diagnosticoExistente.Conclusion = diagnostico.Conclusion;
                    diagnosticoExistente.FechaModificacion = DateTime.Now;

                    var actualizado = await _diagnosticoService.UpdateDiagnostico(diagnostico.Id, diagnosticoExistente);
                    if (!actualizado)
                    {
                        Console.WriteLine("Error al actualizar el diagnóstico.");
                        return StatusCode(500, new { mensaje = "Error al actualizar el diagnóstico." });
                    }

                    diagnostico.Id = diagnosticoExistente.Id;
                }
                else
                {
                    Console.WriteLine("Creando un nuevo diagnóstico.");
                    diagnostico.FechaCreacion = DateTime.Now;
                    await _diagnosticoService.CreateDiagnostico(diagnostico);
                    Console.WriteLine($"Diagnóstico creado con ID: {diagnostico.Id}");
                }

                if (videoEcografia != null && videoEcografia.Length > 0)
                {
                    Console.WriteLine("Iniciando la subida del archivo de video.");
                    var fileName = $"Ecografia_{diagnostico.Id}.avi";
                    var containerName = "videos";

                    try
                    {
                        using var stream = videoEcografia.OpenReadStream();
                        var urlDescarga = await _blobStorageService.UploadFileAsync(stream, fileName, videoEcografia.ContentType, containerName);
                        Console.WriteLine($"Video subido exitosamente. URL: {urlDescarga}");

                        var vidEcografia = new VideoEcografia
                        {
                            Formato = "Avi",
                            Tamaño = $"{videoEcografia.Length / 1024:0.00} KB",
                            UrlDescarga = urlDescarga,
                            IdDiagnostico = diagnostico.Id
                        };

                        await _videoEcografiaService.CreateVideoEcografia(vidEcografia);
                        Console.WriteLine($"VideoEcografia guardado en la base de datos con ID Diagnóstico: {vidEcografia.IdDiagnostico}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error al subir el archivo de video o guardar en la base de datos: {ex.Message}");
                        return StatusCode(500, new { mensaje = "Error al subir el archivo de video o guardar en la base de datos." });
                    }
                }

                var totalImagenesExistentes = (await _imagenRadiologicaService.ObtenerImagenesPorDiagnostico(diagnostico.Id)).Count;
                var imagenIndex = totalImagenesExistentes;

                foreach (var imagen in imagenesRadiologicas)
                {
                    if (imagen.Length > 0)
                    {
                        var fileName = $"Radiologica_{diagnostico.Id}_{imagenIndex}.png";
                        imagenIndex++;

                        var contentType = imagen.ContentType;
                        if (contentType == "application/octet-stream")
                        {
                            contentType = "application/png";
                        }

                        var fileNameImg = $"Radiologica_{diagnostico.Id}_{imagenIndex}.png";
                        var containerName = "imagenes";

                        try
                        {
                            using var stream = imagen.OpenReadStream();
                            var pngUrl = await _blobStorageService.UploadFileAsync(stream, fileNameImg, contentType, containerName);
                            Console.WriteLine($"Imagen PNG subida exitosamente. URL: {pngUrl}");

                            var nuevaImagen = new ImagenRadiologica
                            {
                                Formato = "PNG",
                                Tamaño = $"{imagen.Length / (1024):0.00} KB",
                                UrlDescarga = pngUrl,
                                IdDiagnostico = diagnostico.Id
                            };

                            await _imagenRadiologicaService.GuardarImagenRadiologica(nuevaImagen);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error al subir la imagen o guardar en la base de datos: {ex.Message}");
                            return StatusCode(500, new { mensaje = "Error al subir una o más imágenes radiológicas." });
                        }

                    }
                }

                Console.WriteLine("Diagnóstico y multimedia guardados exitosamente.");
                return Ok(new { mensaje = "Diagnóstico y multimedia guardados exitosamente." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error general: {ex.Message}");
                return StatusCode(500, new { mensaje = "Error al guardar el diagnóstico y/o multimedia." });
            }
        }
    }
}