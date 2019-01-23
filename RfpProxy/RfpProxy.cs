using System;
using System.Buffers.Binary;
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
        private readonly ConcurrentDictionary<Subscription, Subscription> _subscriptions = new ConcurrentDictionary<Subscription, Subscription>();
        private readonly ConcurrentDictionary<RfpIdentifier, RfpConnection> _connections = new ConcurrentDictionary<RfpIdentifier, RfpConnection>();

        public RfpProxy(int listenPort, string ommHost, int ommPort, string socket) 
            : base(listenPort, ommHost, ommPort)
        {
            _socket = socket;
        }

        public override Task RunAsync(CancellationToken cancellationToken)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return base.RunAsync(cancellationToken);
            return RunInternalAsync(cancellationToken);
        }

        protected override CryptedRfpConnection OnClientConnected(TcpClient client, TcpClient server)
        {
            Console.WriteLine("new RFP connection");
            var connection = base.OnClientConnected(client, server);
            connection.Identifier = new RfpIdentifier(new byte[RfpIdentifier.Length]);
            return connection;
        }

        private async Task RunInternalAsync(CancellationToken cancellationToken)
        {
            var socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.IP)
            {
                ReceiveTimeout = 10,
                SendTimeout = 10,
            };
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
            }
            finally
            {
                socket.Close();
            }
        }

        private async Task HandleClientAsync(Socket client, CancellationToken cancellationToken)
        {
            Console.WriteLine("client connected");
            using (client)
            using (var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
            {
                var clientConnection = new ClientConnection(client, OnSubscriptionMessageAsync);
                try
                {
                    using (var stream = new NetworkStream(client, FileAccess.ReadWrite, false))
                    using (var reader = new StreamReader(stream))
                    {
                        await SendAsync(client, Serialize(new Hello("stay connected")), cancellationToken)
                            .ConfigureAwait(false);
                        while (true)
                        {
                            var text = await reader.ReadLineAsync().ConfigureAwait(false);
                            Console.WriteLine($"new subscription: {text}");
                            cancellationToken.ThrowIfCancellationRequested();
                            var msg = Deserialize(text);
                            if (msg.Type == SubscriptionType.End)
                                break;
                            var mac = DecodeHex(msg.Rfp.Filter);
                            var macMask = DecodeHex(msg.Rfp.Mask);
                            var filter = DecodeHex(msg.Message.Filter);
                            var filterMask = DecodeHex(msg.Message.Mask);
                            var subscription = new Subscription(clientConnection, cts, msg.Priority, mac, macMask, filter, filterMask, msg.Type == SubscriptionType.Handle);
                            _subscriptions.TryAdd(subscription, subscription);
                        }
                        await SendAsync(client, Serialize(new Hello("switching protocols")), cancellationToken)
                            .ConfigureAwait(false);
                    }
                }
                catch (Exception ex) when (client.Connected)
                {
                    await SendAsync(client, ex.Message, cancellationToken).ConfigureAwait(false);
                    Console.WriteLine(ex);
                    return;
                }
                await clientConnection.RunAsync(cancellationToken).ConfigureAwait(false);
                cts.Cancel();
            }
            Console.WriteLine("client disconnected");
        }

        private Task OnSubscriptionMessageAsync(OmmMessage message, CancellationToken cancellationToken)
        {
            if (!_connections.TryGetValue(message.Rfp, out var connection))
            {
                throw new InvalidDataException("rfp not found");
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

        private static ReadOnlyMemory<byte> DecodeHex(string hex)
        {
            return BlowFish.HexToByte(hex);
        }

        private static string Serialize<T>(T message)
        {
            return JsonConvert.SerializeObject(message);
        }

        private static Subscribe Deserialize(string message)
        {
            return JsonConvert.DeserializeObject<Subscribe>(message);
        }

        private static Task SendAsync(Socket client, string message, CancellationToken cancellationToken)
        {
            var data = Encoding.UTF8.GetBytes(message + "\n");
            return SendAsync(client, data, cancellationToken);
        }

        private static async Task SendAsync(Socket client, ReadOnlyMemory<byte> data, CancellationToken cancellationToken)
        {
            await client.SendAsync(data, SocketFlags.None, cancellationToken).ConfigureAwait(false);
        }

        private static readonly ReadOnlyMemory<byte> SysInit = new byte[] {0x01, 0x20};
        protected override async Task OnClientMessageAsync(RfpConnection connection, ReadOnlyMemory<byte> data, CancellationToken cancellationToken)
        {
            if (data.Length >= 18 && data.Span.StartsWith(SysInit.Span))
            {
                connection.Identifier = new RfpIdentifier(data.Slice(12,6).ToArray());
                _connections.AddOrUpdate(connection.Identifier, connection, (m, c) => connection);
            }

            foreach (var subscription in _subscriptions.Values.OrderBy(x=>x.Priority))
            {
                try
                {
                    data = await subscription.OnRfpMessageAsync(connection.Identifier, data, cancellationToken).ConfigureAwait(false);
                }
                catch
                {
                    _subscriptions.TryRemove(subscription, out _);
                    subscription.Cancel();
                }
            }
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
                catch
                {
                    _subscriptions.TryRemove(subscription, out _);
                    subscription.Cancel();
                }
            }
            await connection.SendToClientAsync(data, cancellationToken);
        }
    }
}