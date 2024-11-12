namespace ProyectoInformatico.DTOs
{
    public class InformacionCita
    {
        public DateTime FechaCita { get; set; } // Fecha y hora de la cita
        public string Estado { get; set; } // Estado de la cita (pendiente, realizada, cancelada)
        public string Especialista { get; set; } // Nombre del especialista
    }
}
