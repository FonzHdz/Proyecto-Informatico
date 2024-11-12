using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace ProyectoInformatico.Models
{
    public class Cita
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public DateTime FechaCreacion { get; set; }
        public DateTime FechaCita { get; set; }
        public string Estado { get; set; } // "pendiente", "realizada", "cancelada"
        public string IdPaciente { get; set; } // Referencia al Id del paciente
        public int IdEspecialista { get; set; } // Referencia al Id del especialista
    }
}
