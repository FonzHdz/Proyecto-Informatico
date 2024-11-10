using Microsoft.AspNetCore.Mvc;
using ProyectoInformatico.Models;
using ProyectoInformatico.Services;
using System.Threading.Tasks;

namespace ProyectoInformatico.Controllers
{
    public class DoctorController : Controller
    {
        private readonly DoctorService _doctorService;

        public DoctorController(DoctorService doctorService)
        {
            _doctorService = doctorService;
        }

        [HttpGet("acceso-doctor")]
        public IActionResult AccesoDoctor()
        {
            return View("acceso-doctor");
        }

        [HttpGet("registro-doctor")]
        public IActionResult RegistroDoctor()
        {
            return View("registro-doctor");
        }

        [HttpPost("/registro-doctor")]
        public async Task<IActionResult> RegistrarDoctor([FromBody] Doctor doctor)
        {
            try
            {
                var doctorExistente = await _doctorService.GetDoctorByEmail(doctor.Email);
                if (doctorExistente != null)
                {
                    return BadRequest(new { mensaje = "El email ya está registrado" });
                }

                await _doctorService.CreateDoctor(doctor);
                return Ok(new { mensaje = "Doctor registrado con éxito" });
            }
            catch (Exception ex)
            {
                // Registrar el error para facilitar la depuración
                Console.WriteLine($"Error: {ex.Message}");

                // Devolver un mensaje de error JSON controlado
                return StatusCode(500, new { mensaje = "Ocurrió un error en el servidor. Por favor, intente nuevamente." });
            }
        }
    }
}