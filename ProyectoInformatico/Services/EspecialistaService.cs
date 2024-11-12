using MongoDB.Driver;
using ProyectoInformatico.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProyectoInformatico.Services
{
    public class EspecialistaService
    {
        private readonly IMongoCollection<Especialista> _especialistas;

        public EspecialistaService(IMongoClient client)
        {
            var database = client.GetDatabase("ecografia4d");
            _especialistas = database.GetCollection<Especialista>("especialistas");
        }

        public async Task<List<Especialista>> GetAllEspecialistas()
        {
            return await _especialistas.Find(e => true).ToListAsync();
        }

        public async Task<Especialista> GetEspecialistaByIdentificacion(int id)
        {
            return await _especialistas.Find(e => e.Identificacion == id).FirstOrDefaultAsync();
        }

        public async Task CreateEspecialista(Especialista especialista)
        {
            bool identificacionUnica = false;
            do
            {
                especialista.Identificacion = new Random().Next(1000000, 9999999);
                var existente = await _especialistas.Find(e => e.Identificacion == especialista.Identificacion).FirstOrDefaultAsync();
                if (existente == null)
                {
                    identificacionUnica = true;
                }
            } while (!identificacionUnica); 
            
            especialista.Contraseña = BCrypt.Net.BCrypt.HashPassword(especialista.Contraseña);

            await _especialistas.InsertOneAsync(especialista);
        }

        public async Task<bool> UpdateEspecialista(int id, Especialista especialistaIn)
        {
            var result = await _especialistas.ReplaceOneAsync(e => e.Identificacion == id, especialistaIn);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteEspecialista(int id)
        {
            var result = await _especialistas.DeleteOneAsync(e => e.Identificacion == id);
            return result.DeletedCount > 0;
        }

        public async Task<Especialista> GetEspecialistaByCedula(string cedula)
        {
            return await _especialistas.Find(e => e.Cedula == cedula).FirstOrDefaultAsync();
        }
    }
}