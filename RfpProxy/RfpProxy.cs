using System;
using System.Threading;
using System.Threading.Tasks;

namespace RfpProxy
{
    public class RfpProxy : AbstractRfpProxy
    {
        public RfpProxy(int listenPort, string ommHost, int ommPort) 
            : base(listenPort, ommHost, ommPort)
        {
        }

        protected override Task OnClientMessageAsync(RfpConnection connection, ReadOnlyMemory<byte> data, CancellationToken cancellationToken)
        {
            Console.WriteLine($"RFP: {BlowFish.ByteToHex(data.Span)}");
            var send = connection.SendToServerAsync(data, cancellationToken);
            if (send.IsCompletedSuccessfully)
                return Task.CompletedTask;
            return send.AsTask();
        }

        protected override Task OnServerMessageAsync(RfpConnection connection, ReadOnlyMemory<byte> data, CancellationToken cancellationToken)
        {
            Console.WriteLine($"OMM: {BlowFish.ByteToHex(data.Span)}");
            var send = connection.SendToClientAsync(data, cancellationToken);
            if (send.IsCompletedSuccessfully)
                return Task.CompletedTask;
            return send.AsTask();
        }
    }
}