using System;
using System.Buffers.Binary;
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
            var msgType = (MsgType)BinaryPrimitives.ReadUInt16BigEndian(data.Slice(0, 2).Span);
            var msgLen = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(2, 2).Span);
            var msgData = data.Slice(4).Span;
            Console.WriteLine($"RFP: Len:{msgLen} Type:{msgType} Data: {BlowFish.ByteToHex(msgData)}");
            var send = connection.SendToServerAsync(data, cancellationToken);
            if (send.IsCompletedSuccessfully)
                return Task.CompletedTask;
            return send.AsTask();
        }

        protected override Task OnServerMessageAsync(RfpConnection connection, ReadOnlyMemory<byte> data, CancellationToken cancellationToken)
        {
            var msgType = (MsgType)BinaryPrimitives.ReadUInt16BigEndian(data.Slice(0, 2).Span);
            var msgLen = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(2, 2).Span);
            var msgData = data.Slice(4).Span;
            Console.WriteLine($"OMM: Len:{msgLen} Type:{msgType} Data: {BlowFish.ByteToHex(msgData)}");
            var send = connection.SendToClientAsync(data, cancellationToken);
            if (send.IsCompletedSuccessfully)
                return Task.CompletedTask;
            return send.AsTask();
        }
    }
}