using System;
using System.Buffers.Binary;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Mono.Options;
using Newtonsoft.Json;
using RfpProxyLib.Messages;

namespace RfpProxy.ChangeLed
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string socketname = "client.sock";
            bool showHelp = false;
            var options = new OptionSet
            {
                {"s|socket=", "socket path", x => socketname = x},
                {"h|help", "show help", x => showHelp = x != null}
            };
            try
            {
                if (options.Parse(args).Count > 0)
                {
                    showHelp = true;
                }
            }
            catch (OptionException ex)
            {
                Console.Error.Write("rfpproxy.changeled: ");
                Console.Error.WriteLine(ex.Message);
                Console.Error.WriteLine("Try 'dotnet rfpproxy.changeled.dll --help' for more information");
                return;
            }
            if (showHelp)
            {
                options.WriteOptionDescriptions(Console.Error);
                return;
            }
            try
            {
                using (var cts = new CancellationTokenSource())
                using (var socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.IP))
                {
                    Console.CancelKeyPress += (s, e) =>
                    {
                        e.Cancel = true;
                        cts.Cancel();
                        socket.Close();
                    };
                    await socket.ConnectAsync(new UnixDomainSocketEndPoint(socketname), cts.Token);
                    cts.Token.ThrowIfCancellationRequested();
                    await SubscribeAsync(socket, cts.Token).ConfigureAwait(false);
                    await RunAsync(socket, cts.Token).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (SocketException ex) when (ex.SocketErrorCode == SocketError.OperationAborted)
            {
            }
        }

        private static async Task SubscribeAsync(Socket socket, CancellationToken cancellationToken)
        {
            using (var stream = new NetworkStream(socket, false))
            using (var reader = new StreamReader(stream))
            using (var writer = new StreamWriter(stream))
            {
                await ReadAsync(reader, cancellationToken).ConfigureAwait(false);

                var subscribe = new Subscribe
                {
                    Priority = 100,
                    Type = SubscriptionType.Handle,
                    Rfp = new SubscriptionFilter
                    {
                        Filter = "000000000000",
                        Mask = "000000000000",
                    },
                    Message = new SubscriptionFilter
                    {
                        Filter = "010200000403",
                        Mask = "ffff0000ffff",
                    },
                };
                await WriteAsync(writer, subscribe, cancellationToken).ConfigureAwait(false);

                var eos = new Subscribe
                {
                    Type = SubscriptionType.End
                };
                await WriteAsync(writer, eos, cancellationToken).ConfigureAwait(false);

                await ReadAsync(reader, cancellationToken).ConfigureAwait(false);
            }
        }

        private static async Task<string> ReadAsync(StreamReader reader, CancellationToken cancellationToken)
        {
            var msg = await reader.ReadLineAsync().ConfigureAwait(false);
            Console.Write("< ");
            Console.WriteLine(msg);
            cancellationToken.ThrowIfCancellationRequested();
            return msg;
        }

        private static async Task WriteAsync<T>(StreamWriter writer, T data, CancellationToken cancellationToken)
        {
            var msg = JsonConvert.SerializeObject(data);
            await writer.WriteLineAsync(msg).ConfigureAwait(false);
            Console.Write("> ");
            Console.WriteLine(msg);
            cancellationToken.ThrowIfCancellationRequested();
            await writer.FlushAsync().ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
        }
        

        private static async Task RunAsync(Socket socket, CancellationToken cancellationToken)
        {
            var length = new byte[4];
            while (!cancellationToken.IsCancellationRequested)
            {
                var success = await FillBufferAsync(socket, length, cancellationToken).ConfigureAwait(false);
                if (!success) return;

                var msgLength = BinaryPrimitives.ReadUInt32BigEndian(length);
                var msg = new byte[msgLength];
                
                success = await FillBufferAsync(socket, msg, cancellationToken).ConfigureAwait(false);
                if (!success) return;

                var data = OnMessage(msg);
                BinaryPrimitives.WriteUInt32BigEndian(length, (uint) data.Length);
                await socket.SendAsync(length, SocketFlags.None, cancellationToken);
                await socket.SendAsync(data, SocketFlags.None, cancellationToken);
            }
        }

        private static async Task<bool> FillBufferAsync(Socket socket, Memory<byte> buffer, CancellationToken cancellationToken)
        {
            while (buffer.Length > 0)
            {
                var bytesRead = await socket.ReceiveAsync(buffer, SocketFlags.None, cancellationToken);
                if (bytesRead == 0) return false;
                buffer = buffer.Slice(bytesRead);
            }
            return true;
        }

        private static ReadOnlyMemory<byte> OnMessage(Memory<byte> message)
        {
            var direction = message.Span[0];
            var data = message.Slice(1 + 4 + 6);
            if (direction == 0)
            {
                data.Span.Slice(0)[5] = 0x02;
            }
            return message;
        }
    }
}
