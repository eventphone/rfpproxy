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

        ReadOnlyMemory<byte> ParseDNM(RfpConnection connection, ReadOnlyMemory<byte> data)
        {
            return data;
        }

        ReadOnlyMemory<byte> ParseLED(RfpConnection connection, ReadOnlyMemory<byte> data)
        {
            var led_color = (LEDSignal)BinaryPrimitives.ReadUInt16BigEndian(data.Slice(0, 2).Span);
            Console.WriteLine($"  set LED color {led_color}");
            return data;
        }
        
        ReadOnlyMemory<byte> ParseLicenseTimer(RfpConnection connection, ReadOnlyMemory<byte> data)
        {
            var grace_period = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(2, 2).Span);
            Console.WriteLine($"  set grace time: {grace_period} minutes ");
            return data;
        }

        protected override Task OnClientMessageAsync(RfpConnection connection, ReadOnlyMemory<byte> data, CancellationToken cancellationToken)
        {
            var msgType = (MsgType)BinaryPrimitives.ReadUInt16BigEndian(data.Slice(0, 2).Span);
            var msgLen = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(2, 2).Span);
            var msgData = data.Slice(4).Span;
            Console.WriteLine($"RFP: Len:{msgLen,4} Type:{msgType,-22} Data: {BlowFish.ByteToHex(msgData)}");
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
            Console.WriteLine($"OMM: Len:{msgLen,4} Type:{msgType,-22} Data: {BlowFish.ByteToHex(msgData)}");
            switch (msgType)
            {
               case MsgType.DNM:
                   data = ParseDNM(connection, data);
                   break;
               case MsgType.SYS_LED:
                   data = ParseLED(connection, data);
                   break;
               case MsgType.SYS_LICENSE_TIMER:
                   data = ParseLicenseTimer(connection, data);
                   break;
            }
            
            var send = connection.SendToClientAsync(data, cancellationToken);
            if (send.IsCompletedSuccessfully)
                return Task.CompletedTask;
            return send.AsTask();
        }
    }
}