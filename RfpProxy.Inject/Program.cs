using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Mono.Options;
using RfpProxyLib;
using RfpProxyLib.Messages;

namespace RfpProxy.Inject
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string socketname = "client.sock";
            bool showHelp = false;
            string mac = null;
            string message =null;
            bool toOmm = false;
            var options = new OptionSet
            {
                {"s|socket=", "socket path", x => socketname = x},
                {"r|rfp=", "rfp MAC address", x => mac = x},
                {"m|message=", "message to inject", x => message = x},
                {"o|omm", "send to omm instead of rfp", x=>toOmm = x != null},
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
                Console.Error.Write("rfpproxy.inject: ");
                Console.Error.WriteLine(ex.Message);
                Console.Error.WriteLine("Try 'dotnet rfpproxy.inject.dll --help' for more information");
                return;
            }
            if (String.IsNullOrEmpty(mac) || String.IsNullOrEmpty(message))
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
                using (var client = new InjectClient(socketname))
                {
                    Console.CancelKeyPress += (s, e) =>
                    {
                        e.Cancel = true;
                        cts.Cancel();
                        client.Stop();
                    };
                    await client.FinishHandshakeAsync(cts.Token);
                    var rfp = HexEncoding.HexToByte(mac);
                    var data = HexEncoding.HexToByte(message);
                    var direction = toOmm ? MessageDirection.ToOmm : MessageDirection.ToRfp;
                    await client.WriteAsync(direction, 0, new RfpIdentifier(rfp), data, cts.Token);
                    client.Stop();
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

    class InjectClient : ProxyClient
    {
        public InjectClient(string socket):base(socket)
        {   
        }

        protected override Task OnMessageAsync(MessageDirection direction, uint messageId, RfpIdentifier rfp, Memory<byte> data, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
