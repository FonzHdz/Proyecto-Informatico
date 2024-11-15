﻿using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace ProyectoInformatico.Models
{
    public class HistorialMedico
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string IdPaciente { get; set; }
        public List<string> IdDiagnosticos { get; set; }
    }
}
