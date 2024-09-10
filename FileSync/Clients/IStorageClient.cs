using System.Threading.Tasks;

namespace FileSync.Clients
{
    public interface IStorageClient
    {
        Task UploadFile(string filePath);
        Task RemoveFile(string filePath);
        Task RenameFile(string oldFilePath, string filePath);
    }
}
