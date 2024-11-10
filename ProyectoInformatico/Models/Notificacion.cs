using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace ProyectoInformatico.Models
{
    public class Notificacion
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public DateTime FechaEnvio { get; set; }
        public string EmailDestinatario { get; set; }
        public string AsuntoCorreo { get; set; }
        public string MensajeCorreo { get; set; }
        public string IdCita { get; set; } // Referencia al Id de la cita
    }
}
