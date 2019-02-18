using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Mono.Options;
using RfpProxyLib;
using RfpProxyLib.Messages;

namespace RfpProxy.ToggleLed
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
                Console.Error.Write("rfpproxytoggleled: ");
                Console.Error.WriteLine(ex.Message);
                Console.Error.WriteLine("Try 'dotnet rfpproxytoggleled.dll --help' for more information");
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
                using (var client = new ToggleClient(socketname))
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
                    var on = HexEncoding.HexToByte("0102000408010000");
                    var off = HexEncoding.HexToByte("0102000408000000");
                    await client.FinishHandshakeAsync(cts.Token);
                    while (!cts.IsCancellationRequested)
                    {
                        await client.WriteAsync(MessageDirection.ToRfp, 0, rfp, on, cts.Token);
                        await Task.Delay(TimeSpan.FromSeconds(1), cts.Token);
                        await client.WriteAsync(MessageDirection.ToRfp, 0, rfp, off, cts.Token);
                        await Task.Delay(TimeSpan.FromSeconds(1), cts.Token);
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (SocketException ex) when (ex.SocketErrorCode == SocketError.OperationAborted)
            {
            }
        }

        class ToggleClient : ProxyClient
        {
            public ToggleClient(string socket) : base(socket)
            {
            }

            protected override Task OnMessageAsync(MessageDirection direction, uint messageId, RfpIdentifier rfp, Memory<byte> data, CancellationToken cancellationToken)
            {
                return Task.CompletedTask;
            }
        }
    }
}
