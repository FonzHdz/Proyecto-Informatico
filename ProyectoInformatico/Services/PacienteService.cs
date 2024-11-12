using MongoDB.Driver;
using ProyectoInformatico.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProyectoInformatico.Services
{
    public class PacienteService
    {
        private readonly IMongoCollection<Paciente> _pacientes;

        public PacienteService(IMongoClient client)
        {
            var database = client.GetDatabase("ecografia4d");
            _pacientes = database.GetCollection<Paciente>("pacientes");
        }

        public async Task CreatePaciente(Paciente paciente)
        {
            Console.WriteLine($"Contraseña recibida: {paciente.Contraseña} 2");

            paciente.Contraseña = BCrypt.Net.BCrypt.HashPassword(paciente.Contraseña);
            await _pacientes.InsertOneAsync(paciente);
        }

        public async Task<List<Paciente>> GetAllPacientes()
        {
            return await _pacientes.Find(paciente => true).ToListAsync();
        }

        public async Task<Paciente> GetPacienteByCedula(string cedula)
        {
            return await _pacientes.Find(p => p.Cedula == cedula).FirstOrDefaultAsync();
        }

        public async Task<bool> UpdatePaciente(string cedula, Paciente paciente)
        {
            var result = await _pacientes.ReplaceOneAsync(p => p.Cedula == cedula, paciente);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> DeletePaciente(string cedula)
        {
            var result = await _pacientes.DeleteOneAsync(p => p.Cedula == cedula);
            return result.DeletedCount > 0;
        }
    }
}