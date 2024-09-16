using FileSync.Watcher;
using System;
using System.Linq;
using Akka.Actor;
using FileSync.Actors;
using System.Reactive.Linq;
using Akka.Routing;
using FileSync.Clients;
using Microsoft.Extensions.Configuration;
using FileSync.Configuration;
using FileSync.Factories;
using FileSync.Messages;
using System.Collections.Generic;

namespace Runner
{
    class Program
    {
        static void Sync(IList<FileNotificationMessage> messages, IActorRef router)
        {
            messages
                .GroupBy(x => x.OldFullPath)
                .Select(x => x.OrderByDescending(i => i.Timestamp).First())
                .ToList()
                .ForEach(router.Tell);
        }

        static void Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
               .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
               .AddEnvironmentVariables()
               .Build();

            var awsConfig = configuration.GetSection("storage:s3").Get<S3Config>();
            var blobConfig = configuration.GetSection("storage:blob").Get<BlobConfig>();
            var systemName = configuration.GetValue<string>("fileSystem");
            var fileToWatch = configuration.GetValue<string>("fileToWatch");


            using (var system = ActorSystem.Create(systemName))
            {
                var counter = system.ActorOf(CounterActor.Create());
                var azure = system.ActorOf(FileSyncActor.Create(new BlobClient(blobConfig)).WithRouter(new RoundRobinPool(3, new DefaultResizer(1, 10, messagesPerResize: 3))), "AzureBlob");
                var aws = system.ActorOf(FileSyncActor.Create(new S3Client(new AmazonS3ClientFactory(), awsConfig)).WithRouter(new RoundRobinPool(3, new DefaultResizer(1, 10, messagesPerResize: 3))), "S3");
                var actors = new[]
                {
                    azure,
                    aws
                };

                var router = system.ActorOf(Props.Empty.WithRouter(new BroadcastGroup(actors.Select(t => t.Path.ToString()))), "filesync");
                var fw = new FileWatcher(fileToWatch);
                using (fw.Buffer(TimeSpan.FromSeconds(2), 10).Subscribe(e => Sync(e, router), ex => Console.WriteLine(ex.Message), () => Console.WriteLine("Completed")))
                {
                    Console.WriteLine("Press q to finish ...");
                    string input;
                    do
                    {
                        input = Console.ReadLine()?.ToLower();
                        if (input == "count")
                        {
                            counter.Tell(input);
                        }
                    } while (input != "q");
                }
            }
        }
    }
}
