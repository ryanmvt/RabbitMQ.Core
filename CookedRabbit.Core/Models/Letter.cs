namespace CookedRabbit.Core
{
    public class Letter
    {
        public Envelope Envelope { get; set; }
        public ulong LetterId { get; set; }

        public LetterMetadata LetterMetadata { get; set; }
        public byte[] Body { get; set; }

        public Letter() { }

        public Letter(string exchange, string routingKey, byte[] data, LetterMetadata metadata, RoutingOptions routingOptions = null)
        {
            Envelope = new Envelope
            {
                Exchange = exchange,
                RoutingKey = routingKey,
                RoutingOptions = routingOptions ?? DefaultRoutingOptions()
            };
            Body = data;
            LetterMetadata = metadata;
        }

        public Letter(string exchange, string routingKey, byte[] data, string id, RoutingOptions routingOptions = null)
        {
            Envelope = new Envelope
            {
                Exchange = exchange,
                RoutingKey = routingKey,
                RoutingOptions = routingOptions ?? DefaultRoutingOptions()
            };
            Body = data;
            if (!string.IsNullOrWhiteSpace(id))
            { LetterMetadata = new LetterMetadata { Id = id }; }
            else
            { LetterMetadata = new LetterMetadata(); }
        }

        public Letter(string exchange, string routingKey, byte[] data, string id, byte priority)
        {
            Envelope = new Envelope
            {
                Exchange = exchange,
                RoutingKey = routingKey,
                RoutingOptions = DefaultRoutingOptions(priority)
            };
            Body = data;
            if (!string.IsNullOrWhiteSpace(id))
            { LetterMetadata = new LetterMetadata { Id = id }; }
            else
            { LetterMetadata = new LetterMetadata(); }
        }

        public static RoutingOptions DefaultRoutingOptions(byte priority = 0)
        {
            return new RoutingOptions
            {
                DeliveryMode = 2,
                Mandatory = false,
                PriorityLevel = priority
            };
        }

        public Letter Clone()
        {
            return new Letter
            {
                Envelope = new Envelope
                {
                    Exchange = Envelope.Exchange,
                    RoutingKey = Envelope.RoutingKey,
                    RoutingOptions = new RoutingOptions
                    {
                        DeliveryMode = Envelope.RoutingOptions?.DeliveryMode ?? 2,
                        Mandatory = Envelope.RoutingOptions?.Mandatory ?? false,
                        PriorityLevel = Envelope.RoutingOptions?.PriorityLevel ?? 0,
                    }
                },
                LetterMetadata = LetterMetadata
            };
        }
    }
}
