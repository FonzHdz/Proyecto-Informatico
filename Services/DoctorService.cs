using ProyectoInformatico.Models;
using MongoDB.Driver;
using System.Threading.Tasks;
using BCrypt.Net;
using System.Numerics;

namespace ProyectoInformatico.Services
{
    public class DoctorService
    {
        private readonly IMongoCollection<Doctor> _doctores;

        public DoctorService(IMongoClient client)
        {
            var database = client.GetDatabase("ecografia4d");
            _doctores = database.GetCollection<Doctor>("doctores");
        }

        public async Task<Doctor> GetDoctorByEmail(string email)
        {
            return await _doctores.Find(d => d.Email == email).FirstOrDefaultAsync();
        }

        public async Task CreateDoctor(Doctor doctor)
        {
            doctor.Password = BCrypt.Net.BCrypt.HashPassword(doctor.Password);
            await _doctores.InsertOneAsync(doctor);
        }
    }
}
