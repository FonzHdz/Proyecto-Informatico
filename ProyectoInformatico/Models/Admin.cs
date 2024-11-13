using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace ProyectoInformatico.Models
{
    public class Admin
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string Usuario { get; set; }
        public string Contraseña { get; set; }
    }
}