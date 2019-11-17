using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Mono.Options;
using RfpProxyLib;
using RfpProxyLib.Messages;

namespace BusyLed
{
    class Program
    {
        static async Task Main(string[] args)
        {
            bool showHelp = false;
            string socketname = "client.sock";
            var rfps = new List<string>();
            var options = new OptionSet
            {
                {"s|socket=", "socket path", x => socketname = x},
                {"r|rfp=", "rfp MAC address", x =>
                    {
                        if (x.Length != 12)
                            throw new ArgumentException("invalid MAC address");
                        rfps.Add(x);
                    }
                },
                {"h|help", "show help", x => showHelp = x != null},
            };
            try
            {
                if (options.Parse(args).Count > 0 || rfps.Count == 0)
                {
                    showHelp = true;
                }
            }
            catch (OptionException ex)
            {
                Console.Error.Write("busyled: ");
                Console.Error.WriteLine(ex.Message);
                Console.Error.WriteLine("Try 'dotnet busyled.dll --help' for more information");
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
                using (var client = new BusyLedClient(socketname))
                {
                    Console.CancelKeyPress += (s, e) =>
                    {
                        client.ResetLed(rfps);
                        e.Cancel = true;
                        cts.Cancel();
                        client.Stop();
                    };
                    await client.SubscribeAsync(rfps, cts.Token);
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
    }

    class BusyLedClient : ProxyClient
    {
        public BusyLedClient(string socket) : base(socket)
        {
        }

        public async Task SubscribeAsync(ICollection<string> rfps, CancellationToken cancellationToken)
        {
            var filter = "0102 0000 0000 0000";
            var mask = "ffff 0000 0000 0001";
            foreach (var rfp in rfps)
            {
                await AddHandlerAsync(0, rfp, "ffffffffffff", filter, mask, cancellationToken);
            }

            await FinishHandshakeAsync(cancellationToken);

            var setBusy = HexEncoding.HexToByte("0102000402040001");
            foreach (var rfp in rfps)
            {
                var rfpIdentifier = new RfpIdentifier(HexEncoding.HexToByte(rfp));
                await WriteAsync(MessageDirection.ToRfp, 0, rfpIdentifier, setBusy, cancellationToken);
            }
        }

        protected override Task OnMessageAsync(MessageDirection direction, uint messageId, RfpIdentifier rfp, Memory<byte> data, CancellationToken cancellationToken)
        {
            Console.WriteLine("suppressed SYS_LED");
            return WriteAsync(direction, messageId, rfp, Array.Empty<byte>(), cancellationToken);
        }

        public void ResetLed(ICollection<string> rfps)
        {
            var unsetBusy = HexEncoding.HexToByte("0102000402010001");
            foreach (var rfp in rfps)
            {
                var rfpIdentifier = new RfpIdentifier(HexEncoding.HexToByte(rfp));
                var write = WriteAsync(MessageDirection.ToRfp, 0, rfpIdentifier, unsetBusy, CancellationToken.None);
                write.GetAwaiter().GetResult();
            }
        }
    }
}
