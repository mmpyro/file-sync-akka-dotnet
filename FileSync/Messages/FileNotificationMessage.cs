using FileSync.Enums;

namespace FileSync.Messages
{
    public class FileNotificationMessage
    {
        public string FullPath { get; set; }
        public string OldFullPath { get; set; }
        public FileModificationType ModificationType { get; set; }
        public int Timestamp { get; set; }
    }
}
