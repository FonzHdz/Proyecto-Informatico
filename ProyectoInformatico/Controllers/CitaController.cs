using Microsoft.AspNetCore.Mvc;
using ProyectoInformatico.Models;
using ProyectoInformatico.Services;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Authorization;

namespace ProyectoInformatico.Controllers
{
    public class CitaContoller : Controller
    {
        private readonly DiagnosticoService _diagnosticoService;
        private readonly CitaService _citaService;
        private readonly PacienteService _pacienteService;

        public CitaContoller(DiagnosticoService diagnosticoService, CitaService citaService, PacienteService pacienteService)
        {
            _diagnosticoService = diagnosticoService;
            _citaService = citaService;
            _pacienteService = pacienteService;
        }

    }
}