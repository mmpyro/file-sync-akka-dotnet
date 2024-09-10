using Amazon.S3;
using Amazon.S3.Model;
using FileSync.Configuration;
using FileSync.Factories;
using System.IO;
using System.Threading.Tasks;

namespace FileSync.Clients
{
    public class S3Client : IStorageClient
    {
        private readonly IAmazonS3 _client;
        private readonly string _bucketName;

        public S3Client(IAmazonS3ClientFactory amazonS3ClientFactory, S3Config config)
        {
            _bucketName = config.BucketName;
            _client = amazonS3ClientFactory.Create(config);
        }

        public async Task RemoveFile(string filePath)
        {
            var request = new DeleteObjectRequest
            {
                BucketName = _bucketName,
                Key = Path.GetFileName(filePath)
            };
            await _client.DeleteObjectAsync(request);
        }

        public async Task RenameFile(string oldFilePath, string filePath)
        {
            await RemoveFile(oldFilePath);
            await UploadFile(filePath);
        }

        public async Task UploadFile(string filePath)
        {
            var file = new FileInfo(filePath);

            var request = new PutObjectRequest()
            {
                InputStream = file.OpenRead(),
                BucketName = _bucketName,
                Key = Path.GetFileName(filePath),
                AutoCloseStream = true
            };

            await _client.PutObjectAsync(request);
        }
    }
}
