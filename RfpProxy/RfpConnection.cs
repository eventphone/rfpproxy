using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using RfpProxyLib;

namespace RfpProxy
{
    public class RfpConnection
    {
        protected readonly TcpClient Client;
        protected readonly TcpClient Server;

        public RfpConnection(TcpClient client, TcpClient server)
        {
            Client = client;
            Server = server;
        }

        public RfpIdentifier Identifier { get; set; }
        public Guid TraceId { get; } = Guid.NewGuid();

        public virtual ValueTask<int> SendToServerAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken)
        {
            if (data.IsEmpty) return new ValueTask<int>(0);
            return Server.Client.SendAsync(data, SocketFlags.None, cancellationToken);
        }

        public virtual ValueTask<int> SendToClientAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken)
        {
            if (data.IsEmpty) return new ValueTask<int>(0);
            return Client.Client.SendAsync(data, SocketFlags.None, cancellationToken);
        }

        public override string ToString()
        {
            return TraceId.ToString();
        }
    }
}