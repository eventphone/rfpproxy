using System;
using System.Threading;
using System.Threading.Tasks;

namespace RfpProxy
{
    public class Subscription
    {
        private readonly CancellationTokenSource _cts;

        public Subscription(ClientConnection client, CancellationTokenSource cancellationTokenSource, byte priority, Memory<byte> mac, ReadOnlyMemory<byte> macMask, Memory<byte> filter, ReadOnlyMemory<byte> filterMask, bool handle)
        {
            if (mac.Length != RfpIdentifier.Length)
                throw new Exception("invalid mac length");
            if (macMask.Length != RfpIdentifier.Length)
                throw new Exception("invalid mac mask length");
            if (filter.Length != filterMask.Length)
                throw new Exception("filter and filter mask length must match");
            _cts = cancellationTokenSource;

            Client = client;
            Priority = priority;
            var masked = mac.Span;
            for (int i = 0; i < masked.Length; i++)
            {
                masked[i] &= macMask.Span[i];
            }
            Mac = new RfpIdentifier(mac);
            masked = filter.Span;
            for (int i = 0; i < masked.Length; i++)
            {
                masked[i] &= filterMask.Span[i];
            }
            MacMask = macMask;
            Filter = filter;
            FilterMask = filterMask;
            HandleMessage = handle;
        }

        public byte Priority { get; }

        public bool HandleMessage { get; }

        public ClientConnection Client { get; }

        public RfpIdentifier Mac { get; }
        
        public ReadOnlyMemory<byte> MacMask { get; }

        public ReadOnlyMemory<byte> Filter { get; }

        public ReadOnlyMemory<byte> FilterMask { get; }

        private int _nextMessageId = 0;
        private uint NextMessageId()
        {
            var next = Interlocked.Increment(ref _nextMessageId);
            while (next == 0)
            {
                next = Interlocked.Increment(ref _nextMessageId);
            }
            return (uint) next;
        }

        public void Cancel()
        {
            _cts.Cancel();
        }

        public Task<ReadOnlyMemory<byte>> OnRfpMessageAsync(RfpIdentifier identifier, ReadOnlyMemory<byte> data, CancellationToken cancellationToken)
        {
            if (!ShouldHandle(identifier, data))
                return Task.FromResult(data);
            var message = new OmmMessage(MessageDirection.FromRfp, NextMessageId(), identifier, data);
            return OnMessageAsync(message, cancellationToken);
        }

        public Task<ReadOnlyMemory<byte>> OnOmmMessageAsync(RfpIdentifier identifier, ReadOnlyMemory<byte> data, CancellationToken cancellationToken)
        {
            if (!ShouldHandle(identifier, data))
                return Task.FromResult(data);
            var message = new OmmMessage(MessageDirection.FromOmm, NextMessageId(), identifier, data);
            return OnMessageAsync(message, cancellationToken);
        }

        private async Task<ReadOnlyMemory<byte>> OnMessageAsync(OmmMessage message, CancellationToken cancellationToken)
        {
            if (HandleMessage)
            {
                var reply = await Client.HandleAsync(message, cancellationToken).ConfigureAwait(false);
                return reply.Message;
            }
            Client.Send(message, cancellationToken);
            return message.Message;
        }

        private bool ShouldHandle(RfpIdentifier identifier, ReadOnlyMemory<byte> data)
        {
            if (!identifier.Matches(Mac, MacMask.Span))
                return false;
            if (Filter.Length > data.Length)
                return false;
            for (int i = 0; i < Filter.Length; i++)
            {
                var masked = data.Span[i] & FilterMask.Span[i];
                if (masked != Filter.Span[i])
                {
                    return false;
                }
            }
            return true;
        }
    }
}