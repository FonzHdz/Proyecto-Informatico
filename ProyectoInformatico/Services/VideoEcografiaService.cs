using MongoDB.Driver;
using ProyectoInformatico.Models;
using System.Threading.Tasks;

namespace ProyectoInformatico.Services
{
    public class VideoEcografiaService
    {
        private readonly IMongoCollection<VideoEcografia> _videosEcografias;

        public VideoEcografiaService(IMongoClient client)
        {
            var database = client.GetDatabase("ecografia4d");
            _videosEcografias = database.GetCollection<VideoEcografia>("videos_ecografias");
        }

        public async Task CreateVideoEcografia(VideoEcografia video)
        {
            await _videosEcografias.InsertOneAsync(video);
        }
        public async Task<VideoEcografia> GetByDiagnosticoId(string diagnosticoId)
        {
            return await _videosEcografias.Find(v => v.IdDiagnostico == diagnosticoId).FirstOrDefaultAsync();
        }
    }
}