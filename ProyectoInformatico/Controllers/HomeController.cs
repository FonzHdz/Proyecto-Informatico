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
    }
}