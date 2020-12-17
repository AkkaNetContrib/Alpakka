﻿using System;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Streams;
using Akka.Streams.Amqp.V1;
using Akka.Streams.Amqp.V1.Dsl;
using Akka.Streams.Dsl;

namespace Amqp.V1.Source
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var sys = ActorSystem.Create("AMQP-System-Source");
            var materializer = ActorMaterializer.Create(sys);
            var serialization = sys.Serialization;
            var serializer = serialization.FindSerializerForType(typeof(string));

            var address = new Address("127.0.0.1", 5672, "guest", "guest", scheme: "AMQP");

            var queueName = "akka.test";
            var receiverLinkName = "amqp-conn-test-receiver";

            //create source
            var amqpSource = RestartSource.OnFailuresWithBackoff(
                () => {
                    Console.WriteLine("Start/Restart...");
                    return AmqpSource
                        .Create(new AddressSourceSettings<string>(address, receiverLinkName, queueName, 200, serializer));
                },
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(3),
                0.2,
                short.MaxValue
            );

            //run source
            await amqpSource
                .Throttle(1, TimeSpan.FromSeconds(1), 10, ThrottleMode.Shaping)
                .RunForeach(Console.WriteLine, materializer);

            await sys.Terminate();
        }
    }
}
