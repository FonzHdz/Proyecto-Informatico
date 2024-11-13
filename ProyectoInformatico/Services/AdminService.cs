using MongoDB.Driver;
using ProyectoInformatico.Models;
using System.Threading.Tasks;

namespace ProyectoInformatico.Services
{
    public class AdminService
    {
        private readonly IMongoCollection<Admin> _admins;

        public AdminService(IMongoClient client)
        {
            var database = client.GetDatabase("ecografia4d");
            _admins = database.GetCollection<Admin>("admins");
        }

        public async Task<Admin> GetAdminByUsuario(string usuario)
        {
            return await _admins.Find(a => a.Usuario == usuario).FirstOrDefaultAsync();
        }
    }
}