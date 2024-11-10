using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace ProyectoInformatico.Models
{
    public class FirmaEspecialista
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string IdEspecialista { get; set; } // Referencia al Id del especialista
        public string UrlFirma { get; set; } // URL para acceder a la imagen de la firma
    }
}
