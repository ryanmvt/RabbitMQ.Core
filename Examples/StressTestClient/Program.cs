﻿using CookedRabbit.Core.Utils;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace CookedRabbit.Core.StressAndStabilityConsole
{
    public static class Program
    {
        private static Config config;
        private static Topologer topologer;

        private static AutoPublisher apub1;
        private static AutoPublisher apub2;
        private static AutoPublisher apub3;
        private static AutoPublisher apub4;

        private static MessageConsumer con1;
        private static MessageConsumer con2;
        private static MessageConsumer con3;
        private static MessageConsumer con4;

        // Per Publisher
        private const ulong MessageCount = 250_000;
        private const int MessageSize = 1_000;

        public static async Task Main()
        {
            await Console.Out.WriteLineAsync("CookedRabbit.Core StressTest v1.00").ConfigureAwait(false);
            await Console.Out.WriteLineAsync("- StressTest setting everything up...").ConfigureAwait(false);

            var setupFailed = false;
            try
            { await SetupWithSharedChannelPoolAsync().ConfigureAwait(false); }
            catch (Exception ex)
            {
                setupFailed = true;
                await Console.Out.WriteLineAsync($"- StressTest failed with exception {ex.Message}.").ConfigureAwait(false);
            }

            if (!setupFailed)
            {
                await Console.Out.WriteLineAsync("- StressTest starting!").ConfigureAwait(false);

                try
                {
                    await StartStressTestAsync()
                        .ConfigureAwait(false);
                }
                catch (Exception ex)
                { await Console.Out.WriteLineAsync($"- StressTest failed with exception {ex.Message}.").ConfigureAwait(false); }
            }

            await Console.In.ReadLineAsync().ConfigureAwait(false);
        }

        private static async Task SetupWithSharedChannelPoolAsync()
        {
            var sw = Stopwatch.StartNew();
            config = ConfigReader.ConfigFileRead("Config.json");
            topologer = new Topologer(config);

            await topologer
                .ChannelPool
                .InitializeAsync()
                .ConfigureAwait(false);

            apub1 = new AutoPublisher(topologer.ChannelPool);
            apub2 = new AutoPublisher(topologer.ChannelPool);
            apub3 = new AutoPublisher(topologer.ChannelPool);
            apub4 = new AutoPublisher(topologer.ChannelPool);

            await Console.Out.WriteLineAsync("- Creating stress test queues!").ConfigureAwait(false);

            foreach (var kvp in config.LetterConsumerSettings)
            {
                await topologer
                    .DeleteQueueAsync(kvp.Value.QueueName)
                    .ConfigureAwait(false);
            }

            foreach (var kvp in config.LetterConsumerSettings)
            {
                await topologer
                    .CreateQueueAsync(kvp.Value.QueueName, true)
                    .ConfigureAwait(false);
            }

            await apub1.StartAsync().ConfigureAwait(false);
            await apub2.StartAsync().ConfigureAwait(false);
            await apub3.StartAsync().ConfigureAwait(false);
            await apub4.StartAsync().ConfigureAwait(false);

            con1 = new MessageConsumer(topologer.ChannelPool, "Consumer1");
            con2 = new MessageConsumer(topologer.ChannelPool, "Consumer2");
            con3 = new MessageConsumer(topologer.ChannelPool, "Consumer3");
            con4 = new MessageConsumer(topologer.ChannelPool, "Consumer4");
            sw.Stop();

            await Console
                .Out
                .WriteLineAsync($"- Setup has finished in {sw.ElapsedMilliseconds} ms.")
                .ConfigureAwait(false);
        }

        private static async Task StartStressTestAsync()
        {
            var sw = Stopwatch.StartNew();
            var pubSubTask1 = StartPubSubTestAsync(apub1, con1);
            var pubSubTask2 = StartPubSubTestAsync(apub2, con2);
            var pubSubTask3 = StartPubSubTestAsync(apub3, con3);
            var pubSubTask4 = StartPubSubTestAsync(apub4, con4);

            await Task
                .WhenAll(pubSubTask1, pubSubTask2, pubSubTask3, pubSubTask4)
                .ConfigureAwait(false);

            await Console.Out.WriteLineAsync($"- All tests finished in {sw.ElapsedMilliseconds / 60_000.0} minutes!").ConfigureAwait(false);
        }

        private static async Task StartPubSubTestAsync(AutoPublisher autoPublisher, MessageConsumer consumer)
        {
            var publishLettersTask = PublishLettersAsync(autoPublisher, consumer.ConsumerSettings.QueueName, MessageCount);
            var processReceiptsTask = ProcessReceiptsAsync(autoPublisher, MessageCount);
            var consumeMessagesTask = ConsumeMessagesAsync(consumer, MessageCount);

            while (!publishLettersTask.IsCompleted)
            { await Task.Delay(1).ConfigureAwait(false); }

            while (!processReceiptsTask.IsCompleted)
            { await Task.Delay(1).ConfigureAwait(false); }

            await autoPublisher.StopAsync().ConfigureAwait(false);

            while (!consumeMessagesTask.IsCompleted)
            { await Task.Delay(1).ConfigureAwait(false); }
        }

        private static async Task PublishLettersAsync(AutoPublisher apub, string queueName, ulong count)
        {
            var sw = Stopwatch.StartNew();
            for (ulong i = 0; i < count; i++)
            {
                await Task.Run(async () =>
                {
                    var letter = RandomData.CreateSimpleRandomLetter(queueName, MessageSize);
                    letter.Envelope.RoutingOptions.DeliveryMode = 1;
                    letter.LetterId = i;

                    await apub.QueueLetterAsync(letter).ConfigureAwait(false);

                    if (letter.LetterId % 10_000 == 0)
                    {
                        await Console
                            .Out
                            .WriteLineAsync($"- QueueName ({queueName}) is publishing letter {letter.LetterId}")
                            .ConfigureAwait(false);
                    }
                }).ConfigureAwait(false);
            }
            sw.Stop();

            await Console
                .Out
                .WriteLineAsync($"- Finished queueing all letters in {sw.ElapsedMilliseconds / 60_000.0} minutes.")
                .ConfigureAwait(false);
        }

        private static async Task ProcessReceiptsAsync(AutoPublisher apub, ulong count)
        {
            var buffer = apub.GetReceiptBufferReader();
            var receiptCount = 0ul;
            var errorCount = 0ul;

            var sw = Stopwatch.StartNew();
            while (receiptCount + errorCount < count)
            {
                try
                {
                    var receipt = await buffer.ReadAsync().ConfigureAwait(false);
                    if (receipt.IsError)
                    {
                        errorCount++;
                    }

                    //await Task.Delay(1).ConfigureAwait(false);
                }
                catch { errorCount++; break; }

                receiptCount++;
            }
            sw.Stop();

            await Console.Out.WriteLineAsync($"- Finished getting receipts.\r\nReceiptCount: {receiptCount} in {sw.ElapsedMilliseconds / 60_000.0} minutes.\r\nErrorCount: {errorCount}").ConfigureAwait(false);
        }

        private static async Task ConsumeMessagesAsync(MessageConsumer consumer, ulong count)
        {
            var messageCount = 0ul;
            var errorCount = 0ul;

            await consumer
                .StartConsumerAsync(false, true)
                .ConfigureAwait(false);

            var sw = Stopwatch.StartNew();
            try
            {
                while (messageCount + errorCount < count) // TODO: Possible Infinite loop on lost messages.
                {
                    await foreach (var message in consumer.StreamOutMessagesUntilEmptyAsync())
                    {
                        if (message.Ackable)
                        { message.AckMessage(); }

                        //await Task.Delay(1).ConfigureAwait(false);

                        messageCount++;
                    }
                }
            }
            catch
            { errorCount++; }
            sw.Stop();

            await Console.Out.WriteLineAsync($"- Finished consuming messages.\r\nMessageCount: {messageCount} in {sw.ElapsedMilliseconds / 60_000.0} minutes.\r\nErrorCount: {errorCount}").ConfigureAwait(false);
        }
    }
}
