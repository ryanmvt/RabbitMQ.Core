﻿using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;

namespace RabbitMQ.Client.Impl
{
    internal class ConcurrentConsumerDispatcher : IConsumerDispatcher
    {
        private readonly ModelBase model;
        private readonly ConsumerWorkService workService;

        public ConcurrentConsumerDispatcher(ModelBase model, ConsumerWorkService ws)
        {
            this.model = model;
            workService = ws;
            IsShutdown = false;
        }

        public void Quiesce()
        {
            IsShutdown = true;
        }

        public void Shutdown()
        {
            workService.StopWork();
        }

        public void Shutdown(IModel model)
        {
            workService.StopWork(model);
        }

        public bool IsShutdown
        {
            get;
            private set;
        }

        public void HandleBasicConsumeOk(IBasicConsumer consumer,
                                         string consumerTag)
        {
            UnlessShuttingDown(() =>
            {
                try
                {
                    consumer.HandleBasicConsumeOk(consumerTag);
                }
                catch (Exception e)
                {
                    var details = new Dictionary<string, object>()
                    {
                        {"consumer", consumer},
                        {"context",  "HandleBasicConsumeOk"}
                    };
                    model.OnCallbackException(CallbackExceptionEventArgs.Build(e, details));
                }
            });
        }

        public void HandleBasicDeliver(IBasicConsumer consumer,
                                       string consumerTag,
                                       ulong deliveryTag,
                                       bool redelivered,
                                       string exchange,
                                       string routingKey,
                                       IBasicProperties basicProperties,
                                       byte[] body)
        {
            UnlessShuttingDown(() =>
            {
                try
                {
                    consumer.HandleBasicDeliver(consumerTag,
                                                deliveryTag,
                                                redelivered,
                                                exchange,
                                                routingKey,
                                                basicProperties,
                                                body);
                }
                catch (Exception e)
                {
                    var details = new Dictionary<string, object>()
                    {
                        {"consumer", consumer},
                        {"context",  "HandleBasicDeliver"}
                    };
                    model.OnCallbackException(CallbackExceptionEventArgs.Build(e, details));
                }
            });
        }

        public void HandleBasicCancelOk(IBasicConsumer consumer, string consumerTag)
        {
            UnlessShuttingDown(() =>
            {
                try
                {
                    consumer.HandleBasicCancelOk(consumerTag);
                }
                catch (Exception e)
                {
                    var details = new Dictionary<string, object>()
                    {
                        {"consumer", consumer},
                        {"context",  "HandleBasicCancelOk"}
                    };
                    model.OnCallbackException(CallbackExceptionEventArgs.Build(e, details));
                }
            });
        }

        public void HandleBasicCancel(IBasicConsumer consumer, string consumerTag)
        {
            UnlessShuttingDown(() =>
            {
                try
                {
                    consumer.HandleBasicCancel(consumerTag);
                }
                catch (Exception e)
                {
                    var details = new Dictionary<string, object>()
                    {
                        {"consumer", consumer},
                        {"context",  "HandleBasicCancel"}
                    };
                    model.OnCallbackException(CallbackExceptionEventArgs.Build(e, details));
                }
            });
        }

        public void HandleModelShutdown(IBasicConsumer consumer, ShutdownEventArgs reason)
        {
            // the only case where we ignore the shutdown flag.
            try
            {
                consumer.HandleModelShutdown(model, reason);
            }
            catch (Exception e)
            {
                var details = new Dictionary<string, object>()
                    {
                        {"consumer", consumer},
                        {"context",  "HandleModelShutdown"}
                    };
                model.OnCallbackException(CallbackExceptionEventArgs.Build(e, details));
            }
        }

        private void UnlessShuttingDown(Action fn)
        {
            if (!IsShutdown)
            {
                Execute(fn);
            }
        }

        private void Execute(Action fn)
        {
            workService.AddWork(model, fn);
        }
    }
}
