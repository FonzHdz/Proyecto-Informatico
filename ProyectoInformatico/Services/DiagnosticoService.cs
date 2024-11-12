using MongoDB.Driver;
using ProyectoInformatico.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProyectoInformatico.Services
{
    public class DiagnosticoService
    {
        private readonly IMongoCollection<Diagnostico> _diagnosticos;

        public DiagnosticoService(IMongoClient client)
        {
            var database = client.GetDatabase("ecografia4d");
            _diagnosticos = database.GetCollection<Diagnostico>("diagnosticos");
        }

        public async Task CreateDiagnostico(Diagnostico diagnostico)
        {
            await _diagnosticos.InsertOneAsync(diagnostico);
        }

        public async Task<List<Diagnostico>> GetDiagnosticosByPacienteId(string idPaciente)
        {
            return await _diagnosticos.Find(d => d.IdPaciente == idPaciente).ToListAsync();
        }

        public async Task<List<Diagnostico>> GetDiagnosticosByEspecialistaId(int idEspecialista)
        {
            return await _diagnosticos.Find(d => d.IdEspecialista == idEspecialista).ToListAsync();
        }
        public async Task<Diagnostico> GetDiagnosticoById(string id)
        {
            Console.WriteLine($"Buscando diagnóstico con Id: {id}"); // Log del ID recibido

            var diagnostico = await _diagnosticos.Find(d => d.Id == id).FirstOrDefaultAsync();

            if (diagnostico == null)
            {
                Console.WriteLine("Diagnóstico no encontrado."); // Log si no se encuentra el diagnóstico
            }
            else
            {
                Console.WriteLine($"Diagnóstico encontrado: {diagnostico.Descripcion}"); // Log si se encuentra el diagnóstico
            }

            return diagnostico;
        }

        public async Task<bool> UpdateDiagnostico(string id, Diagnostico diagnostico)
        {
            var result = await _diagnosticos.ReplaceOneAsync(d => d.Id == id, diagnostico);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteDiagnostico(string id)
        {
            var result = await _diagnosticos.DeleteOneAsync(d => d.Id == id);
            return result.DeletedCount > 0;
        }
    }
}