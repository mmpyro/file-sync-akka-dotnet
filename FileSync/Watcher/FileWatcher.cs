using FileSync.Enums;
using FileSync.Messages;
using System;
using System.IO;

namespace FileSync.Watcher
{

    public class FileWatcher : IObservable<FileNotificationMessage>, IDisposable
    {
        private readonly FileSystemWatcher _fw;
        private IObserver<FileNotificationMessage> _observer;
        public FileWatcher(string path)
        {
            _fw = new FileSystemWatcher(path)
            {
                EnableRaisingEvents = true
            };

            _fw.Created += (s, e) =>
            {
                _observer?.OnNext(new FileNotificationMessage
                {
                    ModificationType = FileModificationType.CREATED,
                    FullPath = e.FullPath
                });
            };

            _fw.Changed += (s, e) =>
            {
                _observer?.OnNext(new FileNotificationMessage
                {
                    ModificationType = FileModificationType.CHANGED,
                    FullPath = e.FullPath
                });
            };

            _fw.Deleted += (s, e) =>
            {
                _observer?.OnNext(new FileNotificationMessage
                {
                    ModificationType = FileModificationType.DELETED,
                    FullPath = e.FullPath
                });
            };

            _fw.Renamed += (s, e) =>
            {
                _observer?.OnNext(new FileNotificationMessage
                {
                    ModificationType = FileModificationType.RENAMED,
                    OldFullPath = e.OldFullPath,
                    FullPath = e.FullPath
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
