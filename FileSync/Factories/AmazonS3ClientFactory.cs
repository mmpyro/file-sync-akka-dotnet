using Amazon.S3;
using FileSync.Configuration;

namespace FileSync.Factories
{
    public interface IAmazonS3ClientFactory
    {
        IAmazonS3 Create(S3Config config);
    }

    public class AmazonS3ClientFactory : IAmazonS3ClientFactory
    {
        public IAmazonS3 Create(S3Config config)
        {
            var s3Config = new AmazonS3Config
            {
                AuthenticationRegion = config.AuthenticationRegion
            };

            if (config.Minio)
            {
                s3Config.ForcePathStyle = true;
                s3Config.ServiceURL = config.ServiceURL;
            }
            return new AmazonS3Client(config.Accesskey, config.SecretKey, s3Config);
        }
    }
}
