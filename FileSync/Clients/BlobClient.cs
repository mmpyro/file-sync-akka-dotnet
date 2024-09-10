using System.Threading.Tasks;
using System.IO;
using Azure.Storage.Blobs;
using FileSync.Configuration;

namespace FileSync.Clients
{

    public class BlobClient : IStorageClient
    {
        private readonly BlobContainerClient _client;

        public BlobClient(BlobConfig config)
        {
            _client = new BlobContainerClient(config.ConnectionString, config.BucketName);
            _client.CreateIfNotExists();
        }

        public async Task RemoveFile(string filePath)
        {
            var blobName = Path.GetFileName(filePath);
            var blobClient = _client.GetBlobClient(blobName);
            await blobClient.DeleteIfExistsAsync();
        }

        public async Task RenameFile(string oldFilePath, string filePath)
        {
            await RemoveFile(oldFilePath);
            await UploadFile(filePath);    
        }

        public async Task UploadFile(string filePath)
        {
            var blobName = Path.GetFileName(filePath);
            using var uploadFileStream = File.OpenRead(filePath);
            var blobClient = _client.GetBlobClient(blobName);
            await blobClient.UploadAsync(uploadFileStream, overwrite: true);
        }
    }
}
