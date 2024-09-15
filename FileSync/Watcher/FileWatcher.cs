using FileSync.Enums;
using FileSync.Messages;
using FileSync.Utils;
using System;
using System.IO;

namespace FileSync.Watcher
{

    public class FileWatcher : IObservable<FileNotificationMessage>, IDisposable
    {
        private readonly FileSystemWatcher _fw;
        private IObserver<FileNotificationMessage> _observer;

        private FileNotificationMessage CreateMessage(FileSystemEventArgs e, FileModificationType modificationType)
        {
            return new FileNotificationMessage
            {
                ModificationType = modificationType,
                FullPath = e.FullPath,
                OldFullPath = e.FullPath,
                Timestamp = EpochDateTime.UtcNow()
            };
        }

        public FileWatcher(string path)
        {
            _fw = new FileSystemWatcher(path)
            {
                EnableRaisingEvents = true
            };

            _fw.Created += (s, e) =>
            {
                _observer?.OnNext(CreateMessage(e, FileModificationType.CREATED));
            };

            _fw.Changed += (s, e) =>
            {
                _observer?.OnNext(CreateMessage(e, FileModificationType.CHANGED));
            };

            _fw.Deleted += (s, e) =>
            {
                _observer?.OnNext(CreateMessage(e, FileModificationType.DELETED));
            };

            _fw.Renamed += (s, e) =>
            {
                _observer?.OnNext(new FileNotificationMessage
                {
                    ModificationType = FileModificationType.RENAMED,
                    OldFullPath = e.OldFullPath,
                    FullPath = e.FullPath,
                    Timestamp = EpochDateTime.UtcNow()
                });
            };

            _fw.Error += (s, e) =>
            {
                _observer?.OnError(e.GetException());
            };
        }

        public void Dispose()
        {
            _observer?.OnCompleted();
        }

        public IDisposable Subscribe(IObserver<FileNotificationMessage> observer)
        {
            _observer = observer;
            return this;
        }
    }
}
