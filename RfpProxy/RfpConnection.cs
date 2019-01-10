using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

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

        public virtual ValueTask<int> SendToServerAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken)
        {
            return Server.Client.SendAsync(data, SocketFlags.None, cancellationToken);
        }

        public virtual ValueTask<int> SendToClientAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken)
        {
            return Client.Client.SendAsync(data, SocketFlags.None, cancellationToken);
        }
    }
}