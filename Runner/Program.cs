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

namespace Runner
{
    class Program
    {
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
                //var router = system.ActorOf(FileSyncActor.Create(new BlobClient(blobConfig)).WithRouter(new RoundRobinPool(1, new DefaultResizer(1, 10))), "AzureBlob");

                var actors = new[]
                {
                    system.ActorOf(FileSyncActor.Create(new S3Client(new AmazonS3ClientFactory(), awsConfig)), "S3"),
                    system.ActorOf(FileSyncActor.Create(new BlobClient(blobConfig)), "AzureBlob")
                };

                var router = system.ActorOf(Props.Empty.WithRouter(new BroadcastGroup(actors.Select(t => t.Path.ToString()))), "filesync");
                var fw = new FileWatcher(fileToWatch);
                using (fw.Throttle(TimeSpan.FromSeconds(.5)).Subscribe(e => { Console.WriteLine(e.FullPath); router.Tell(e); }, ex => Console.WriteLine(ex.Message), () => Console.WriteLine("Completed")))
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
