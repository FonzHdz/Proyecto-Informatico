namespace ProyectoInformatico.DTOs
{
    public class InformacionCita
    {
        public string Id { get; set; }
        public DateTime FechaCita { get; set; }
        public string Estado { get; set; }
        public string Especialista { get; set; }
        public string Paciente { get; set; }
    }
}
