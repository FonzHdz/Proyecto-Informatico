using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace ProyectoInformatico.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View("inicio");
        }

        public IActionResult Contacto()
        {
            return View("contacto");
        }

        [HttpGet("acceso-denegado")]
        public IActionResult AccesoDenegado()
        {
            return View("acceso-denegado");
        }

        [HttpGet("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }
    }
}