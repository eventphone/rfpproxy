using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Mono.Options;
using RfpProxyLib;
using RfpProxy.AaMiDe.Nwk;
using RfpProxy.AaMiDe.Nwk.InformationElements;
using RfpProxyLib.Messages;

namespace RfpProxy.AVM
{
    class Program
    {
        static async Task Main(string[] args)
        {
            bool showHelp = false;
            string socketname = "client.sock";
            var options = new OptionSet
            {
                {"s|socket=", "socket path", x => socketname = x},
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
                Console.Error.Write("avm: ");
                Console.Error.WriteLine(ex.Message);
                Console.Error.WriteLine("Try 'dotnet avm.dll --help' for more information");
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
                using (var client = new ReplaceSignalClient(socketname))
                {
                    Console.CancelKeyPress += (s, e) =>
                    {
                        e.Cancel = true;
                        cts.Cancel();
                        client.Stop();
                    };
                    await client.SubscribeAsync(cts.Token);
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

    public class ReplaceSignalClient : ProxyClient
    {
        private static readonly HashSet<ushort> EMCs = new HashSet<ushort>
        {
            04563,
            09615,
            10596,
            11640,
            11942, //TODO from our blog post, but also flagged as gigaset in GURU3
            12574,
        };

        private static readonly byte SignalType = 0b11100100;

        public ReplaceSignalClient(string socket):base(socket)
        {
        }
        
        public async Task SubscribeAsync(CancellationToken cancellationToken)
        {
            //                                            v SETUP
            //                       v Lc               v CC v IE Portable Identity
            //               v DNM     v LcDataInd|LcDataReq   v IE Length
            var filter = "0301 0000 7905 00000000 0000 0305 0500";
            var mask = "  ffff 0000 fffc 00000000 0000 ffff ff00";
            await AddHandlerAsync(0, "000000000000", "000000000000", filter, mask, cancellationToken);
        }

        protected override Task OnMessageAsync(MessageDirection direction, uint messageId, RfpIdentifier rfp, Memory<byte> data, CancellationToken cancellationToken)
        {
            try
            {
                var ies = data.Slice(14);
                while (!ies.IsEmpty)
                {
                    var current = ies.Span[0];
                    if (current > SignalType)
                        break;
                    if (current >= 128)
                    {
                        //fixed length
                        if (current >= 224)
                        {
                            if (ies.Length < 2)
                                break;
                            if (current == SignalType)
                            {
                                ies.Span[1] = 0x41;
                                Console.WriteLine("patched alerting pattern");
                            }
                            ies = ies.Slice(2);
                        }
                        else
                        {
                            ies = ies.Slice(1);
                        }
                    }
                    else
                    {
                        if (ies.Length < 2)
                            break;
                        var length = ies.Span[1];
                        ies = ies.Slice(2);
                        if (current == (byte) NwkVariableLengthElementType.PortableIdentity)
                        {
                            //portable identity
                            var ie = new NwkIePortableIdentity(ies);
                            if (ie.IdentityType == NwkIePortableIdentity.PortableIdentityType.IPUI)
                            {
                                if (ie.Ipui.Put == NwkIePortableIdentity.IPUITypeCoding.O && ies.Length >= 5)
                                {
                                    var span = ies.Slice(3).Span;
                                    var emc = (ushort)((span[0] & 0xf) << 12 | (span[1] << 4) | (span[2] >> 4));
                                    if (!EMCs.Contains(emc))
                                        break;
                                }
                            }
                        }
                        if (ies.Length < length)
                            break;
                        ies = ies.Slice(length);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return WriteAsync(direction, messageId, rfp, data, cancellationToken);
        }
    }
}
