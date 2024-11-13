using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ProyectoInformatico.Services
{
    public class BlobStorageService
    {
        private readonly BlobServiceClient _blobServiceClient;

        public BlobStorageService(string connectionString)
        {
            _blobServiceClient = new BlobServiceClient(connectionString);
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, string containerName)
        {
            var blobContainerClient = _blobServiceClient.GetBlobContainerClient(containerName);

            await blobContainerClient.CreateIfNotExistsAsync();
            await blobContainerClient.SetAccessPolicyAsync(PublicAccessType.Blob);

            var blobClient = blobContainerClient.GetBlobClient(fileName);

            var blobHttpHeaders = new BlobHttpHeaders { ContentType = contentType };
            await blobClient.UploadAsync(fileStream, blobHttpHeaders);

            return blobClient.Uri.ToString();
        }
    }
}