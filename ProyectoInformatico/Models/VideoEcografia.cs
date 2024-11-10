using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace ProyectoInformatico.Models
{
    public class VideoEcografia
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string Formato { get; set; } // Por ejemplo: "avi"
        public string Tamaño { get; set; }
        public string Resolucion { get; set; }
        public string UrlDescarga { get; set; } // URL para acceder al video
        public string IdDiagnostico { get; set; } // Referencia al Id del diagnóstico
    }
}
