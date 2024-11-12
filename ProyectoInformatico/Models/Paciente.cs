using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;

namespace ProyectoInformatico.Models
{
    public class Paciente
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public string Cedula { get; set; }

        public bool EsExtranjero { get; set; }
        public string Nacionalidad { get; set; }
        public string Correo { get; set; }
        public string Contraseña { get; set; } // Usar hashing para almacenar contraseñas de forma segura
        public string Nombre { get; set; }
        public string Genero { get; set; }
        public string Telefono { get; set; }
        public string Direccion { get; set; }
        public string Departamento { get; set; }
        public string Ciudad { get; set; }
        public DateTime FechaNacimiento { get; set; }
        public string TipoSangre { get; set; }
        public int SemanasEmbarazo { get; set; }
        public DateTime FechaUltimaEcografia { get; set; }
        public string EstadoCivil { get; set; }
        public string Alergias { get; set; }
    }
}
