using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Mono.Options;
using RfpProxy.AaMiDe.Nwk.InformationElements;
using RfpProxyLib;
using RfpProxyLib.Messages;

namespace RfpProxy.CompressIPUI
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
                Console.Error.Write("compressipui: ");
                Console.Error.WriteLine(ex.Message);
                Console.Error.WriteLine("Try 'dotnet compressipui.dll --help' for more information");
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
                using (var client = new ReplaceIpuiClient(socketname))
                {
                    Console.CancelKeyPress += (s, e) =>
                    {
                        e.Cancel = true;
                        cts.Cancel();
                        client.Stop();
                    };
                    await client.SubscribeAsync(cts.Token);
                    await client.RunAsync(cts.Token);
                    cts.Cancel();
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

    class ReplaceIpuiClient : ProxyClient
    {
        private static readonly HashSet<ushort> EMCs = new HashSet<ushort>
        {
            0x3014, // Motorla / SGW Global
            0x1603, //T-Sinus 302
            0x1627, //T-Sinus 302
            0x1671, //T-Sinus 302
            0x177a, //T-Sinus 302
            0x246d, //Belgacom
            0x1714, //Swissvoice
        };

        public ReplaceIpuiClient(string socket):base(socket)
        {
        }

        public async Task SubscribeAsync(CancellationToken cancellationToken)
        {
            //                       v Lc                    v IE Portable Identity
            //               v DNM     v LcDataInd|LcDataReq   v IE Length
            var filter = "0301 0000 7907 00000000 0000 0000 0508";
            var mask = "  ffff 0000 fffc 00000000 0000 0000 ff0f";
            await AddHandlerAsync(0, "000000000000", "000000000000", filter, mask, cancellationToken);
        }

        protected override Task OnMessageAsync(MessageDirection direction, uint messageId, RfpIdentifier rfp, Memory<byte> data, CancellationToken cancellationToken)
        {
            try
            {
                var length = data.Span[15];
                if (length == 8)
                {
                    var iedata = data.Slice(16);
                    var ie = new NwkIePortableIdentity(iedata);
                    if (ie.IdentityType == NwkIePortableIdentity.PortableIdentityType.IPUI)
                    {
                        var ipui = ie.Ipui;
                        if (ipui.Put == NwkIePortableIdentity.IPUITypeCoding.O)
                        {
                            if (direction == MessageDirection.FromOmm)
                            {
                                var emc = (ushort)(ipui.Number >> 20);
                                if (EMCs.Contains(emc))
                                {
                                    Console.WriteLine(data.ToHex());
                                    var span = iedata.Span;
                                    span[2] = (byte)(0x10 | (emc >> 12));
                                    span[3] = (byte)(0xff & (emc >> 4));
                                    span[4] = (byte)((0xf & emc) << 4);
                                    span[4] |= (byte) (ipui.Number >> 16 & 0xf);
                                    BinaryPrimitives.WriteUInt16BigEndian(span.Slice(5), (ushort) ipui.Number);
                                    span[7] = 0x00;
                                    Console.WriteLine($"shifted   {data.ToHex()}");
                                }
                            }
                            else
                            {
                                var emc = (ushort)(ipui.Number >> 28);
                                if (EMCs.Contains(emc))
                                {
                                    Console.WriteLine(data.ToHex());
                                    //00000080 b0100301 400fdf
                                    var span = iedata.Span;
                                    span[2] = 0x10;
                                    span[3] = (byte)(emc >> 12);
                                    BinaryPrimitives.WriteUInt32BigEndian(span.Slice(4), (uint) (ipui.Number >> 8));
                                    Console.WriteLine($"unshifted {data.ToHex()}");
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Console.WriteLine(data.ToHex());
            }
            return WriteAsync(direction, messageId, rfp, data, cancellationToken);
        }
    }
}
