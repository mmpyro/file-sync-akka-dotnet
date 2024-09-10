using Akka.Actor;
using FileSync.Clients;
using FileSync.Enums;
using FileSync.Messages;
using System;
using System.Threading.Tasks;

namespace FileSync.Actors
{

    public class FileSyncActor : ReceiveActor, IWithUnboundedStash
    {
        private readonly IStorageClient _client;

        public IStash Stash { get; set; }

        private class SyncMessage
        {
            public SyncMessage(BlobSyncedStatus status, string fullPath)
            {
                Status = status;
                FullPath = fullPath;
            }

            public BlobSyncedStatus Status { get; private set; }
            public string FullPath { get; private set; }
        }

        public FileSyncActor(IStorageClient client)
        {
            _client = client;
            Ready();
        }

        private void Ready()
        {
            Receive<FileNotificationMessage>(msg =>
            {
                Console.WriteLine($"Actor {Self.Path} received notification {msg.ModificationType} for file {msg.FullPath}");
                var self = Self;
                UpdateFile(msg).ContinueWith(t =>
                {
                    if (t.IsCompletedSuccessfully)
                    {
                        return new SyncMessage(BlobSyncedStatus.SUCCESS, msg.FullPath); ;
                    }
                    else if (t.IsFaulted)
                    {
                        Console.WriteLine(t.Exception);
                        return new SyncMessage(BlobSyncedStatus.FAILURE, msg.FullPath);
                    }
                    return new SyncMessage(BlobSyncedStatus.UNKNOWN, msg.FullPath);
                }, TaskContinuationOptions.AttachedToParent & TaskContinuationOptions.ExecuteSynchronously)
                .PipeTo(self);
                Become(Busy);
            });

            Receive<CountMessageRequest>(_ =>
            {
                Sender.Tell(new CountMessageResponse
                {
                    Id = Self.Path.Uid.ToString()
                });
            });
        }

        private void Busy()
        {
            Receive<SyncMessage>(m =>
            {
                Console.WriteLine($"Actor {Self.Path} sync file {m.FullPath} with status: {m.Status}.");
                BecomeReady();
            });
            ReceiveAny(o => Stash.Stash());
        }

        private void BecomeReady()
        {
            Stash.UnstashAll();
            Become(Ready);
        }

        private Task UpdateFile(FileNotificationMessage msg)
        {
            return msg.ModificationType switch
            {
                FileModificationType.CREATED => _client.UploadFile(msg.FullPath),
                FileModificationType.CHANGED => _client.UploadFile(msg.FullPath),
                FileModificationType.RENAMED => _client.RenameFile(msg.OldFullPath, msg.FullPath),
                FileModificationType.DELETED => _client.RemoveFile(msg.FullPath),
                _ => Task.CompletedTask
            };
        }

        public static Props Create(IStorageClient client)
        {
            return Props.Create(() => new FileSyncActor(client));
        }
    }
}
