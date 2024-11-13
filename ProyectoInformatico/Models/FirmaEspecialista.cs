using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace ProyectoInformatico.Models
{
    public class FirmaEspecialista
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string Formato { get; set; }
        public string Tamaño { get; set; }
        public string UrlDescarga { get; set; }
        public string IdEspecialista { get; set; }
    }
}
