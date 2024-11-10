using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace ProyectoInformatico.Models
{
    public class ImagenRadiologica
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string Formato { get; set; } // Por ejemplo: "DICOM"
        public string Tamaño { get; set; }
        public string Resolucion { get; set; }
        public string UrlDescarga { get; set; } // URL para acceder a la imagen
        public string IdDiagnostico { get; set; } // Referencia al Id del diagnóstico
    }
}
