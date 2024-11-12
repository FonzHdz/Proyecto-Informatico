using MongoDB.Driver;
using ProyectoInformatico.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProyectoInformatico.Services
{
    public class CitaService
    {
        private readonly IMongoCollection<Cita> _citas;

        public CitaService(IMongoClient client)
        {
            var database = client.GetDatabase("ecografia4d");
            _citas = database.GetCollection<Cita>("citas");
        }

        public async Task CreateCita(Cita cita)
        {
            await _citas.InsertOneAsync(cita);
        }

        public async Task<List<Cita>> GetCitasByPacienteId(string idPaciente)
        {
            return await _citas.Find(c => c.IdPaciente == idPaciente).ToListAsync();
        }

        public async Task<List<Cita>> GetCitasByEspecialistaId(int idEspecialista)
        {
            return await _citas.Find(c => c.IdEspecialista == idEspecialista).ToListAsync();
        }

        public async Task<Cita> GetCitaById(string id)
        {
            return await _citas.Find(c => c.Id == id).FirstOrDefaultAsync();
        }

        public async Task<bool> UpdateCita(string id, Cita cita)
        {
            var result = await _citas.ReplaceOneAsync(c => c.Id == id, cita);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteCita(string id)
        {
            var result = await _citas.DeleteOneAsync(c => c.Id == id);
            return result.DeletedCount > 0;
        }
    }
}