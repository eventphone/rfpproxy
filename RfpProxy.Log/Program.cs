using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Mono.Options;
using RfpProxyLib.Messages;
using RfpProxy.Log.Messages;
using RfpProxyLib;

namespace RfpProxy.Log
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string socketname = "client.sock";
            string mac = "000000000000";
            string rfpMmask = "000000000000";
            string filter = String.Empty;
            string filterMask = String.Empty;
            bool showHelp = false;
            bool logRaw = false;
            bool unknown = false;
            var options = new OptionSet
            {
                {"s|socket=", "socket path", x => socketname = x},
                {"r|rfp=", "rfp MAC address", x => mac = x},
                {"rm|rfpmask=", "rfp mask", x => rfpMmask = x},
                {"f|filter=", "filter", x => filter = x},
                {"fm|filtermask=", "filter mask", x => filterMask = x},
                {"raw", "log raw packets", x => logRaw = x != null},
                {"u|unknown", "log only packets with unknown payload", x => unknown = x != null},
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
                using (var client = new LogClient(socketname, logRaw, unknown))
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
                    await client.AddListenAsync(mac, rfpMmask, filter, filterMask, cts.Token).ConfigureAwait(false);
                    await client.RunAsync(cts.Token).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (SocketException ex) when (ex.SocketErrorCode == SocketError.OperationAborted)
            {
            }
        }

        class LogClient : ProxyClient
        {
            private readonly bool _logRaw;
            private readonly bool _unknown;
            private readonly Dictionary<RfpIdentifier, AaMiDeReassembler> _reassemblers = new Dictionary<RfpIdentifier, AaMiDeReassembler>();

            public LogClient(string socket, bool logRaw, bool unknown) : base(socket)
            {
                _logRaw = logRaw;
                _unknown = unknown;
            }

            protected override Task OnMessageAsync(MessageDirection direction, uint messageId, RfpIdentifier rfp, Memory<byte> data, CancellationToken cancellationToken)
            {
                if (data.IsEmpty)
                    return Task.CompletedTask;
                AaMiDeMessage message;
                if (!_reassemblers.TryGetValue(rfp, out var reassembler))
                {
                    reassembler = new AaMiDeReassembler();
                    _reassemblers.Add(rfp, reassembler);
                }
                string prefix;
                if (direction == MessageDirection.FromOmm)
                {
                    prefix = "OMM:";
                }
                else
                {
                    prefix = "RFP:";
                }
                try
                {
                    message = AaMiDeMessage.Create(data, reassembler);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{DateTime.Now:yyyy/MM/dd HH:mm:ss.fff} {prefix}{rfp} Cannot parse {data.ToHex()}");
                    Console.WriteLine(ex);
                    return Task.CompletedTask;
                }
                if (_unknown && !message.HasUnknown)
                    return Task.CompletedTask;
                Console.Write($"{DateTime.Now:yyyy/MM/dd HH:mm:ss.fff} {prefix}{rfp} ");
                message.Log(Console.Out);
                Console.WriteLine();
                if (_logRaw)
                {
                    Console.Write("\t");
                    int i = 0;
                    var span = data.Span;
                    for (; i < span.Length-4; i += 4)
                    {
                        Console.Write(span.Slice(i, 4).ToHex());
                        Console.Write(' ');
                    }
                    Console.WriteLine(span.Slice(i).ToHex());
                }
                return Task.CompletedTask;
            }
        }
    }
}