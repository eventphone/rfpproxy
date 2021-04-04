using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RfpProxyLib;
using RfpProxyLib.Messages;

namespace RfpProxy
{
    public class RfpProxy : TransparentRfpProxy
    {
        private readonly string _socket;
        private readonly string _ommConf;
        private readonly ConcurrentDictionary<Subscription, Subscription> _subscriptions = new ConcurrentDictionary<Subscription, Subscription>();
        private readonly ConcurrentDictionary<RfpIdentifier, RfpConnection> _connections = new ConcurrentDictionary<RfpIdentifier, RfpConnection>();

        public RfpProxy(int listenPort, string ommHost, int ommPort, string socket, string ommConf) 
            : base(listenPort, ommHost, ommPort)
        {
            _socket = socket;
            _ommConf = ommConf;
        }

        public override Task RunAsync(CancellationToken cancellationToken)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return base.RunAsync(cancellationToken);
            return RunInternalAsync(cancellationToken);
        }

        protected override CryptedRfpConnection OnClientConnected(TcpClient client, TcpClient server)
        {
            Console.WriteLine($"new RFP connection from {client.Client.RemoteEndPoint}");
            var connection = base.OnClientConnected(client, server);
            connection.Identifier = new RfpIdentifier(new byte[RfpIdentifier.Length]);
            return connection;
        }

        protected override void OnClientDisconnected(CryptedRfpConnection client)
        {
            _connections.TryRemove(client.Identifier, out _);
        }

        private async Task RunInternalAsync(CancellationToken cancellationToken)
        {
            var socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.IP)
            {
                ReceiveTimeout = 10,
                SendTimeout = 10,
            };
            try
            {
                File.Delete(_socket);
            }
            catch (FileNotFoundException)
            {
                //ignore, that's what we want
            }
            socket.Bind(new UnixDomainSocketEndPoint(_socket));
            socket.Listen(5);
            try
            {
                var accept = socket.AcceptAsync();
                var tasks = new HashSet<Task>
                {
                    accept,
                    Task.Delay(Timeout.Infinite, cancellationToken),
                    base.RunAsync(cancellationToken)
                };
                Console.WriteLine("interception socket up & running");
                while (!cancellationToken.IsCancellationRequested)
                {
                    await Task.WhenAny(tasks).ConfigureAwait(false);
                    if (cancellationToken.IsCancellationRequested) return;
                    if (accept.IsCompleted)
                    {
                        var client = await accept.ConfigureAwait(false);
                        tasks.Add(HandleClientAsync(client, cancellationToken));
                        tasks.Remove(accept);
                        accept = socket.AcceptAsync();
                        tasks.Add(accept);
                    }
                    else
                    {
                        foreach (var task in tasks.Where(x=>x.IsCompleted).ToList())
                        {
                            try
                            {
                                await task.ConfigureAwait(false);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("Client connection failed");
                                Console.WriteLine(ex);
                            }
                            tasks.Remove(task);
                        }
                    }
                }
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine("cancelled in RfpProxy.RunInternalAsync");
            }
            finally
            {
                socket.Close();
            }
        }

        private async Task HandleClientAsync(Socket client, CancellationToken cancellationToken)
        {
            Console.WriteLine("client connected");
            client.SendTimeout = 1000;
            using (client)
            using (var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
            {
                var clientConnection = new ClientConnection(client, OnSubscriptionMessageAsync);
                var subscriptions = new List<Subscription>();
                clientConnection.Subscribed += (s, e) =>
                {
                    var msg = e.Subscription;
                    var mac = HexEncoding.HexToByte(msg.Rfp.Filter);
                    var macMask = HexEncoding.HexToByte(msg.Rfp.Mask);
                    var filter = HexEncoding.HexToByte(msg.Message.Filter);
                    var filterMask = HexEncoding.HexToByte(msg.Message.Mask);
                    var subscription = new Subscription(clientConnection, cts, msg.Priority, mac, macMask, filter, filterMask, msg.Type == SubscriptionType.Handle);
                    subscriptions.Add(subscription);
                    _subscriptions.TryAdd(subscription, subscription);
                };
                await clientConnection.RunAsync(cancellationToken).ConfigureAwait(false);
                cts.Cancel();
                foreach (var subscription in subscriptions)
                {
                    _subscriptions.TryRemove(subscription, out _);
                }
            }
            Console.WriteLine("client disconnected");
        }

        private Task OnSubscriptionMessageAsync(OmmMessage message, CancellationToken cancellationToken)
        {
            if (!_connections.TryGetValue(message.Rfp, out var connection))
            {
                return Task.CompletedTask;
            }
            switch (message.Direction)
            {
                case MessageDirection.FromOmm:
                    return OnServerMessageAsync(connection, message.Message, cancellationToken);
                case MessageDirection.ToOmm:
                    return OnClientMessageAsync(connection, message.Message, cancellationToken);
                default:
                    throw new ArgumentOutOfRangeException(nameof(message.Direction), "invalid message direction");
            }
        }

        private static readonly ReadOnlyMemory<byte> SysInit = new byte[] {0x01, 0x20};
        protected override async Task OnClientMessageAsync(RfpConnection connection, ReadOnlyMemory<byte> data, CancellationToken cancellationToken)
        {
            if (data.Length >= 18 && data.Span.StartsWith(SysInit.Span))
            {
                _connections.AddOrUpdate(connection.Identifier, connection, (m, c) => connection);
            }

            foreach (var subscription in _subscriptions.Values.OrderBy(x=>x.Priority))
            {
                try
                {
                    data = await subscription.OnRfpMessageAsync(connection.Identifier, data, cancellationToken).ConfigureAwait(false);
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    _subscriptions.TryRemove(subscription, out _);
                    try
                    {
                        subscription.Cancel();
                    }
                    catch(ObjectDisposedException){}
                }
            }
            if (!data.IsEmpty)
                await connection.SendToServerAsync(data, cancellationToken).ConfigureAwait(false);
        }

        protected override async Task OnServerMessageAsync(RfpConnection connection, ReadOnlyMemory<byte> data, CancellationToken cancellationToken)
        {
            foreach (var subscription in _subscriptions.Values.OrderBy(x=>x.Priority))
            {
                try
                {
                    data = await subscription.OnOmmMessageAsync(connection.Identifier, data, cancellationToken)
                        .ConfigureAwait(false);
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    _subscriptions.TryRemove(subscription, out _);
                    try
                    {
                        subscription.Cancel();
                    }
                    catch(ObjectDisposedException){}
                }
            }
            if (!data.IsEmpty)
                await connection.SendToClientAsync(data, cancellationToken).ConfigureAwait(false);
        }

        protected override async Task<Memory<byte>> GetRfpaAsync(CryptedRfpConnection connection, CancellationToken cancellationToken)
        {
            using (var reader = new OmmConfReader(_ommConf))
            {
                var rfp = await reader.GetValueAsync("RFP", "mac", connection.Identifier.ToString().ToUpper(), cancellationToken).ConfigureAwait(false);
                if (rfp is null) return Memory<byte>.Empty;
                var id = rfp["id"];
                var rfpa = await reader.GetValueAsync("RFPA", "id", id, cancellationToken).ConfigureAwait(false);
                if (rfpa is null) return Memory<byte>.Empty;
                var key = rfpa[1];
                return HexEncoding.HexToByte(key);
            }
        }

        protected override async Task<string> GetRootPasswordHashAsync(CancellationToken cancellationToken)
        {
            using (var reader = new OmmConfReader(_ommConf))
            {
                var user = await reader.GetValueAsync("UA", "user", "root", cancellationToken);
                return user?["password"];
            }
        }
    }
}