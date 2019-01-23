using System;
using System.Buffers.Binary;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace RfpProxy
{
    public enum MessageDirection:byte
    {
        FromOmm = 0,
        ToRfp = FromOmm,
        FromRfp = 1,
        ToOmm = FromRfp
    }

    public class OmmMessage
    {
        private readonly SemaphoreSlim _receivedReply = new SemaphoreSlim(0,1);
        private OmmMessage _reply;

        public OmmMessage(MessageDirection direction, uint messageId, RfpIdentifier rfp, ReadOnlyMemory<byte> message)
        {
            Direction = direction;
            MessageId = messageId;
            Rfp = rfp;
            Message = message;
        }

        public OmmMessage(ReadOnlyMemory<byte> data)
        {
            var span = data.Span;
            Direction = (MessageDirection) span[0];
            MessageId = BinaryPrimitives.ReadUInt32BigEndian(span.Slice(1));
            data = data.Slice(5);
            Rfp = new RfpIdentifier(data.Slice(0, RfpIdentifier.Length));
            data = data.Slice(RfpIdentifier.Length);
            Message = data;
        }

        public bool IsReply => MessageId != 0;

        public uint MessageId { get; }

        public ReadOnlyMemory<byte> Message { get; }

        public MessageDirection Direction { get; }

        public OmmMessage Reply
        {
            get { return _reply; }
            set
            {
                _reply = value;
                _receivedReply.Release();
            }
        }

        public RfpIdentifier Rfp { get; }

        public async Task SendAsync(Socket socket, CancellationToken cancellationToken)
        {
            var header = new byte[4 + 1 + 4 + RfpIdentifier.Length];
            BinaryPrimitives.WriteInt32BigEndian(header.AsSpan(), 1 + 4 + RfpIdentifier.Length + Message.Length);
            header[4] = (byte) Direction;
            BinaryPrimitives.WriteUInt32BigEndian(header.AsSpan(5), MessageId);
            Rfp.CopyTo(header.AsMemory().Slice(4 + 1 + 4));
            await socket.SendAsync(header, SocketFlags.None, cancellationToken).ConfigureAwait(false);
            await socket.SendAsync(Message, SocketFlags.None, cancellationToken).ConfigureAwait(false);
        }

        public async Task<OmmMessage> WaitForReplyAsync(CancellationToken cancellationToken)
        {
            var replied = await _receivedReply.WaitAsync(TimeSpan.FromMilliseconds(10), cancellationToken)
                .ConfigureAwait(false);
            if (!replied) 
                throw new TimeoutException("client did not answer within 10ms");
            return _reply;
        }
    }
}