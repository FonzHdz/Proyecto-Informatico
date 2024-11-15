﻿using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace ProyectoInformatico.Models
{
    public class Especialista
    {
        [BsonId]
        [BsonRepresentation(BsonType.Int32)]
        public int Identificacion { get; set; }

        public string Especialidad { get; set; }
        public string Cedula { get; set; }
        public string Contraseña { get; set; }
        public string Nombre { get; set; }
        public string Genero { get; set; }
        public string Telefono { get; set; }
        public string Direccion { get; set; }
        public string Departamento { get; set; }
        public string Ciudad { get; set; }
        public DateTime FechaNacimiento { get; set; }
    }
}
