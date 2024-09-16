using Akka.Actor;
using FileSync.Messages;
using System;
using System.Collections.Generic;
using System.Text;


namespace FileSync.Actors
{
    public class CounterActor : ReceiveActor, IWithTimers
    {
        private readonly HashSet<string> _set = [];

        public CounterActor()
        {
            Receive<string>(msg =>
            {
               if(msg.ToLower() == "count")
                {
                    _set.Clear();
                    Context.ActorSelection("/user/*/*").Tell(new CountMessageRequest());
                    Timers.StartSingleTimer("counting-timer", new FinishCountingMessage(), TimeSpan.FromSeconds(1));
                }
            });

            Receive<CountMessageResponse>(msg =>
            {
                _set.Add(msg.Id);
            });

            Receive<FinishCountingMessage>(_ =>
            {
                var sb = new StringBuilder();
                sb.AppendLine($"Number of user Actors is: {_set.Count}");
                sb.AppendLine("-----------------------------");
                foreach(var item in _set)
                {
                    sb.AppendLine(item);
                }
                sb.AppendLine("-----------------------------");
                Console.WriteLine(sb.ToString());
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
