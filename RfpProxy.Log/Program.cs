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

namespace RfpProxy.Log
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
                Console.Error.Write("rfpproxy.log: ");
                Console.Error.WriteLine(ex.Message);
                Console.Error.WriteLine("Try 'dotnet rfpproxy.log.dll --help' for more information");
            }
            if (showHelp)
            {
                options.WriteOptionDescriptions(Console.Error);
                return;
            }
            using (var cts = new CancellationTokenSource())
            {
                Console.CancelKeyPress += (s, e) =>
                {
                    e.Cancel = true; 
                    cts.Cancel();
                };
                using (var socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.IP))
                {
                    socket.Bind(new UnixDomainSocketEndPoint(socketname));
                    await SubscribeAsync(socket, cts.Token).ConfigureAwait(false);
                    await LogAsync(socket, cts.Token).ConfigureAwait(false);
                }
            }
        }

        private static async Task SubscribeAsync(Socket socket, CancellationToken cancellationToken)
        {
            using (var stream = new NetworkStream(socket, false))
            using (var reader = new StreamReader(stream))
            using (var writer = new StreamWriter(stream))
            {
                var syn = await reader.ReadLineAsync().ConfigureAwait(false);
                Console.WriteLine(syn);
                cancellationToken.ThrowIfCancellationRequested();

                var subscribe = new Subscribe
                {
                    Priority = 255,
                    Type = SubscriptionType.Listen,
                    Rfp = new SubscriptionFilter
                    {
                        Filter = "000000000000",
                        Mask = "000000000000",
                    },
                    Message = new SubscriptionFilter
                    {
                        Filter = String.Empty,
                        Mask = String.Empty,
                    },
                };
                await writer.WriteLineAsync(JsonConvert.SerializeObject(subscribe)).ConfigureAwait(false);
                cancellationToken.ThrowIfCancellationRequested();
                var eos = new Subscribe
                {
                    Type = SubscriptionType.End
                };
                await writer.WriteLineAsync(JsonConvert.SerializeObject(eos)).ConfigureAwait(false);
                cancellationToken.ThrowIfCancellationRequested();
                var ack = await reader.ReadLineAsync().ConfigureAwait(false);
                Console.WriteLine(ack);
                cancellationToken.ThrowIfCancellationRequested();
            }
        }

        private static async Task LogAsync(Socket socket, CancellationToken cancellationToken)
        {
            var length = new byte[4];
            while (!cancellationToken.IsCancellationRequested)
            {
                await FillBufferAsync(socket, length, cancellationToken).ConfigureAwait(false);

                var msgLength = BinaryPrimitives.ReadInt32BigEndian(length);
                var msg = new byte[msgLength];
                
                await FillBufferAsync(socket, msg, cancellationToken).ConfigureAwait(false);

                OnMessage(msg);
            }
        }

        private static async Task FillBufferAsync(Socket socket, Memory<byte> buffer, CancellationToken cancellationToken)
        {
            while (buffer.Length > 0)
            {
                var bytesRead = await socket.ReceiveAsync(buffer, SocketFlags.None, cancellationToken);
                if (bytesRead == 0) break;
                buffer = buffer.Slice(bytesRead);
            }
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
            var msgType = (MsgType)BinaryPrimitives.ReadUInt16BigEndian(data.Slice(0, 2).Span);
            var msgLen = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(2, 2).Span);
            var msgData = data.Slice(4).Span;
            Console.WriteLine($"RFP:{ByteToHex(identifier)} Len:{msgLen,4} Type:{msgType,-22} Data: {ByteToHex(msgData)}");
        }

        private static void OnServerMessage(ReadOnlySpan<byte> identifier, ReadOnlyMemory<byte> data)
        {
            var msgType = (MsgType)BinaryPrimitives.ReadUInt16BigEndian(data.Slice(0, 2).Span);
            var msgLen = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(2, 2).Span);
            var msgData = data.Slice(4).Span;
            Console.WriteLine($"OMM:{ByteToHex(identifier)} Len:{msgLen,4} Type:{msgType,-22} Data: {ByteToHex(msgData)}");
            switch (msgType)
            {
                case MsgType.DNM:
                    ParseDNM(identifier, data);
                    break;
                case MsgType.SYS_LED:
                    ParseLED(identifier, data);
                    break;
                case MsgType.SYS_LICENSE_TIMER:
                    ParseLicenseTimer(identifier, data);
                    break;
            }
        }

        private static void ParseDNM(ReadOnlySpan<byte> identifier, ReadOnlyMemory<byte> data)
        {
            return;
        }

        private static void ParseLED(ReadOnlySpan<byte> identifier, ReadOnlyMemory<byte> data)
        {
            var color_byte = (data.Slice(5,1).Span)[0];
            var led_color = (LEDSignal)color_byte;
            Console.WriteLine($"  set LED color {led_color}");
        }

        private static void ParseLicenseTimer(ReadOnlySpan<byte> identifier, ReadOnlyMemory<byte> data)
        {
            var grace_period = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(6, 2).Span);
            Console.WriteLine($"  set grace time: {grace_period} minutes ");
        }

        private static string ByteToHex(ReadOnlySpan<byte> bytes)
        {
            StringBuilder s = new StringBuilder(bytes.Length*2);
            foreach (byte b in bytes)
                s.Append(b.ToString("x2"));
            return s.ToString();
        }
    }
}