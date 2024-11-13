﻿using MongoDB.Driver;
using ProyectoInformatico.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProyectoInformatico.Services
{
    public class ImagenRadiologicaService
    {
        private readonly IMongoCollection<ImagenRadiologica> _imagenesRadiologicas;

        public ImagenRadiologicaService(IMongoClient client)
        {
            var database = client.GetDatabase("ecografia4d");
            _imagenesRadiologicas = database.GetCollection<ImagenRadiologica>("imagenes_radiologicas");
        }

        public async Task GuardarImagenRadiologica(ImagenRadiologica imagen)
        {
            await _imagenesRadiologicas.InsertOneAsync(imagen);
        }

        public async Task<List<ImagenRadiologica>> ObtenerImagenesPorDiagnostico(string diagnosticoId)
        {
            return await _imagenesRadiologicas
                .Find(imagen => imagen.IdDiagnostico == diagnosticoId)
                .ToListAsync();
        }
        public async Task<ImagenRadiologica> ObtenerImagenPorIdAsync(string imagenId)
        {
            return await _imagenesRadiologicas
                .Find(imagen => imagen.Id == imagenId)
                .FirstOrDefaultAsync();
        }

        public async Task EliminarImagenPorIdAsync(string imagenId)
        {
            await _imagenesRadiologicas.DeleteOneAsync(imagen => imagen.Id == imagenId);
        }

        public async Task EliminarImagenesPorDiagnosticoAsync(string diagnosticoId)
        {
            await _imagenesRadiologicas.DeleteManyAsync(imagen => imagen.IdDiagnostico == diagnosticoId);
        }
    }
}