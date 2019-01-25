using System;
using System.Buffers;
using System.Buffers.Binary;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Mono.Options;
using Newtonsoft.Json;
using RfpProxyLib.Messages;
using RfpProxy.Log.Messages;

namespace RfpProxy.Log
{
    class Program
    {
        private static bool _logRaw = false;
        static async Task Main(string[] args)
        {
            string socketname = "client.sock";
            string mac = "000000000000";
            string rfpMmask = "000000000000";
            string filter = String.Empty;
            string filterMask = String.Empty;
            bool showHelp = false;
            var options = new OptionSet
            {
                {"s|socket=", "socket path", x => socketname = x},
                {"r|rfp=", "rfp MAC address", x => mac = x},
                {"rm|rfpmask=", "rfp mask", x=>rfpMmask = x},
                {"f|filter=", "filter", x => filter = x},
                {"fm|filtermask=", "filter mask", x=>filterMask = x},
                {"raw", "log raw packets", x=>_logRaw = x != null},
                {"h|help", "show help", x => showHelp = x != null},
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
                Console.Error.Write("rfpproxy.log: ");
                Console.Error.WriteLine(ex.Message);
                Console.Error.WriteLine("Try 'dotnet rfpproxy.log.dll --help' for more information");
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
                    await socket.ConnectAsync(new UnixDomainSocketEndPoint(socketname));
                    cts.Token.ThrowIfCancellationRequested();
                    await SubscribeAsync(socket, mac, rfpMmask, filter, filterMask, cts.Token).ConfigureAwait(false);
                    await LogAsync(socket, cts.Token).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (SocketException ex) when (ex.SocketErrorCode == SocketError.OperationAborted)
            {
            }
        }

        private static async Task SubscribeAsync(Socket socket, string mac, string mask, string filter, string filterMask, CancellationToken cancellationToken)
        {
            using (var stream = new NetworkStream(socket, false))
            using (var reader = new StreamReader(stream))
            using (var writer = new StreamWriter(stream))
            {
                await ReadAsync(reader, cancellationToken).ConfigureAwait(false);

                var subscribe = new Subscribe
                {
                    Priority = 255,
                    Type = SubscriptionType.Listen,
                    Rfp = new SubscriptionFilter
                    {
                        Filter = mac,
                        Mask = mask,
                    },
                    Message = new SubscriptionFilter
                    {
                        Filter = filter,
                        Mask = filterMask,
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

        private static async Task LogAsync(Socket socket, CancellationToken cancellationToken)
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

                OnMessage(msg);
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

        private static void OnMessage(ReadOnlyMemory<byte> message)
        {
            var span = message.Span;
            var direction = span[0];
            var identifier = span.Slice(5, 6);
            var data = message.Slice(1 + 4 + 6);
            if (direction == 0)
            {
                OnServerMessage(identifier, data);
            }
            else
            {
                OnClientMessage(identifier, data);
            }
        }

        private static void OnClientMessage(ReadOnlySpan<byte> identifier, ReadOnlyMemory<byte> data)
        {
            var message = AaMiDeMessage.Create(data);
            Console.Write($"RFP:{AaMiDeMessage.ByteToHex(identifier)} ");
            if (_logRaw)
            {
                Console.WriteLine(AaMiDeMessage.ByteToHex(data.Span));
                Console.Write($"RFP:{AaMiDeMessage.ByteToHex(identifier)} ");
            }
            message.Log(Console.Out);
            Console.WriteLine();
        }

        private static void OnServerMessage(ReadOnlySpan<byte> identifier, ReadOnlyMemory<byte> data)
        {
            var message = AaMiDeMessage.Create(data);
            Console.Write($"OMM:{AaMiDeMessage.ByteToHex(identifier)} ");
            if (_logRaw)
            {
                Console.WriteLine(AaMiDeMessage.ByteToHex(data.Span));
                Console.Write($"RFP:{AaMiDeMessage.ByteToHex(identifier)} ");
            }
            message.Log(Console.Out);
            Console.WriteLine();
        }
    }
}