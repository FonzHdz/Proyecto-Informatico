using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace ProyectoInformatico.Models
{
    public class Diagnostico
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string Descripcion { get; set; }
        public string Resultados { get; set; }
        public string Observaciones { get; set; }
        public string Conclusion { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime FechaModificacion { get; set; }
        public int IdEspecialista { get; set; } // Referencia al Id del especialista
        public string IdPaciente { get; set; } // Referencia al Id del paciente
    }
}
