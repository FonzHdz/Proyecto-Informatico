using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace ProyectoInformatico.Models
{
    public class Especialista
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string Especialidad { get; set; } // Ejemplo: "Ginecología" o "Radiología"
        public int Cedula { get; set; }
        public string Contraseña { get; set; } // Usar hashing para almacenar contraseñas de forma segura
        public string Nombre { get; set; }
        public string Genero { get; set; }
        public int Telefono { get; set; }
        public string Direccion { get; set; }
        public string Departamento { get; set; }
        public string Ciudad { get; set; }
        public DateTime FechaNacimiento { get; set; }
    }
}
