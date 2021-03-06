using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace RabbitMQ.Client.Unit
{
    public class TestEndpointResolver : IEndpointResolver
    {
        private readonly IEnumerable<AmqpTcpEndpoint> _endpoints;
        public TestEndpointResolver(IEnumerable<AmqpTcpEndpoint> endpoints)
        {
            _endpoints = endpoints;
        }

        public IEnumerable<AmqpTcpEndpoint> All()
        {
            return _endpoints;
        }
    }

    class TestEndpointException : Exception
    {
        public TestEndpointException(string message) : base(message)
        {
        }

        public TestEndpointException() : base()
        {
        }

        public TestEndpointException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    public class TestIEndpointResolverExtensions
    {
        [Test]
        public void SelectOneShouldReturnDefaultWhenThereAreNoEndpoints()
        {
            var ep = new TestEndpointResolver(new List<AmqpTcpEndpoint>());
            Assert.IsNull(ep.SelectOne<AmqpTcpEndpoint>((x) => null));
        }

        [Test]
        public void SelectOneShouldRaiseThrownExceptionWhenThereAreOnlyInaccessibleEndpoints()
        {
            var ep = new TestEndpointResolver(new List<AmqpTcpEndpoint> { new AmqpTcpEndpoint() });
            AggregateException thrown = Assert.Throws<AggregateException>(() => ep.SelectOne<AmqpTcpEndpoint>((x) => { throw new TestEndpointException("bananas"); }));
            Assert.That(thrown.InnerExceptions, Has.Exactly(1).TypeOf<TestEndpointException>());
        }

        [Test]
        public void SelectOneShouldReturnFoundEndpoint()
        {
            var ep = new TestEndpointResolver(new List<AmqpTcpEndpoint> { new AmqpTcpEndpoint() });
            Assert.IsNotNull(ep.SelectOne<AmqpTcpEndpoint>((e) => e));
        }
    }
}
