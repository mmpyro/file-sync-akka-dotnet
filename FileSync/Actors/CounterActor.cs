using Akka.Actor;
using FileSync.Messages;
using System;
using System.Collections.Generic;


namespace FileSync.Actors
{
    public class CounterActor : ReceiveActor, IWithTimers
    {
        private readonly HashSet<string> _set = new();

        public CounterActor()
        {
            Receive<string>(msg =>
            {
               if(msg.ToLower() == "count")
                {
                    Context.ActorSelection("/user/*").Tell(new CountMessageRequest());
                    Timers.StartSingleTimer("counting-timer", new FinishCountingMessage(), TimeSpan.FromSeconds(.5));
                }
            });

            Receive<CountMessageResponse>(msg =>
            {
                _set.Add(msg.Id);
            });

            Receive<FinishCountingMessage>(_ =>
            {
                Console.WriteLine($"Number of user Actors is: {_set.Count}");
                _set.Clear();
            });

            Receive<CountMessageRequest>(_ => { });
        }

        public ITimerScheduler Timers { get; set; }

        public static Props Create()
        {
            return Props.Create(() => new CounterActor());
        }
    }
}
