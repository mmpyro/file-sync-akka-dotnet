namespace FileSync.Configuration
{
    public class S3Config
    {
        public string SecretKey { get; set; }
        public string Accesskey { get; set; }
        public string BucketName { get; set; }
        public string AuthenticationRegion { get; set; } = "us-east-1";
        public bool Minio { get; set; } = false;
        public string ServiceURL { get; set; }
    }
}
