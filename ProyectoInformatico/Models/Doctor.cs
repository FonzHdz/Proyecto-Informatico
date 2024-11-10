using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ProyectoInformatico.Models
{
    public class Doctor
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string Nombre { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }
}