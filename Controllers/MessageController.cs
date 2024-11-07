using Microsoft.AspNetCore.Mvc;

namespace PruebaProyectoInformatico.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessageController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetMessage()
        {
            return Ok(new { message = "Hello from the ASP.NET Core backend!" });
        }
    }
}