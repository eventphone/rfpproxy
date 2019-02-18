using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Mono.Options;
using RfpProxyLib;
using RfpProxyLib.Messages;

namespace RfpProxy.Traffic
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string socketname = "client.sock";
            bool showHelp = false;
            string mac = null;
            var options = new OptionSet
            {
                {"s|socket=", "socket path", x => socketname = x},
                {"r|rfp=", "rfp MAC address", x => mac = x},
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
                Console.Error.Write("rfpproxytraffic: ");
                Console.Error.WriteLine(ex.Message);
                Console.Error.WriteLine("Try 'dotnet rfpproxytraffic.dll --help' for more information");
                return;
            }
            if (String.IsNullOrEmpty(mac))
            {
                showHelp = true;
            }
            if (showHelp)
            {
                options.WriteOptionDescriptions(Console.Error);
                return;
            }
            try
            {
                using (var cts = new CancellationTokenSource())
                using (var client = new TrafficClient(socketname))
                {
                    Console.CancelKeyPress += (s, e) =>
                    {
                        e.Cancel = true;
                        cts.Cancel();
                        client.Stop();
                    };
                    client.Log += (s, e) =>
                    {
                        Console.Write(e.Direction == LogDirection.Read ? "< " : "> ");
                        Console.WriteLine(e.Message);
                    };
                    var rfp = new RfpIdentifier(HexEncoding.HexToByte(mac));
                    await client.AddHandlerAsync(0, mac, "ffffffffffff", "010e000cac141701", "ffffffffffffffff", cts.Token);
                    await client.FinishHandshakeAsync(cts.Token);
                    await client.WriteAsync(MessageDirection.ToRfp, 0, rfp, HexEncoding.HexToByte("010e000cac1417010000000000000000"), cts.Token);
                    await client.RunAsync(cts.Token);
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (SocketException ex) when (ex.SocketErrorCode == SocketError.OperationAborted)
            {
            }
        }

        class TrafficClient : ProxyClient
        {
            private byte[] _ping;

            public TrafficClient(string socket) : base(socket)
            {
                _ping = HexEncoding.HexToByte("010e000c" +
                                              "ac141701" +
                                              "0000000000000000");
            }
            
            protected override async Task OnMessageAsync(MessageDirection direction, uint messageId, RfpIdentifier rfp, Memory<byte> data, CancellationToken cancellationToken)
            {
                await WriteAsync(direction, messageId, rfp, ReadOnlyMemory<byte>.Empty, cancellationToken);
                if (direction == MessageDirection.ToOmm)
                {
                    await WriteAsync(MessageDirection.ToRfp, 0, rfp, _ping, cancellationToken);
                }
            }
        }
    }
}
