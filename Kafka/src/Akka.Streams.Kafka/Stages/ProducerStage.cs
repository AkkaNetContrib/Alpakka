﻿using System;
using System.Threading.Tasks;
using Akka.Streams.Kafka.Messages;
using Akka.Streams.Kafka.Settings;
using Akka.Streams.Stage;
using Confluent.Kafka;

namespace Akka.Streams.Kafka.Stages
{
    internal sealed class ProducerStage<K, V> : GraphStageWithMaterializedValue<FlowShape<ProduceMessage<K, V>, Task<DeliveryReport<K, V>>>, Task>
    {
        public ProducerSettings<K, V> Settings { get; }
        public bool CloseProducerOnStop { get; }
        public Func<IProducer<K, V>> ProducerProvider { get; }
        public Inlet<ProduceMessage<K, V>> In { get; } = new Inlet<ProduceMessage<K, V>>("kafka.producer.in");
        public Outlet<Task<DeliveryReport<K, V>>> Out { get; } = new Outlet<Task<DeliveryReport<K, V>>>("kafka.producer.out");

        public ProducerStage(
            ProducerSettings<K, V> settings,
            bool closeProducerOnStop,
            Func<IProducer<K, V>> producerProvider)
        {
            Settings = settings;
            CloseProducerOnStop = closeProducerOnStop;
            ProducerProvider = producerProvider;

            Shape = new FlowShape<ProduceMessage<K, V>, Task<DeliveryReport<K, V>>>(In, Out);
        }

        public override FlowShape<ProduceMessage<K, V>, Task<DeliveryReport<K, V>>> Shape { get; }

        public override ILogicAndMaterializedValue<Task> CreateLogicAndMaterializedValue(Attributes inheritedAttributes)
        {
            var completion = new TaskCompletionSource<NotUsed>();
            return new LogicAndMaterializedValue<Task>(new ProducerStageLogic<K, V>(this, inheritedAttributes, completion), completion.Task);
        }
    }
}