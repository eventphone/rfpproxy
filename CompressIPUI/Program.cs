﻿using System;
using System.Buffers.Binary;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Mono.Options;
using RfpProxyLib;
using RfpProxyLib.AaMiDe.Nwk.InformationElements;
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
        public ReplaceIpuiClient(string socket):base(socket)
        {
        }

        public async Task SubscribeAsync(CancellationToken cancellationToken)
        {
            //                       v Lc                    v IE Portable Identity
            //               v DNM     v LcDataInd|LcDataReq   v IE Length
            var filter = "0301 0000 7907 00000000 0000 0000 0500";
            var mask = "  ffff 0000 fffc 00000000 0000 0000 ff00";
            await AddHandlerAsync(0, "000000000000", "000000000000", filter, mask, cancellationToken);
        }

        protected override Task OnMessageAsync(MessageDirection direction, uint messageId, RfpIdentifier rfp, Memory<byte> data, CancellationToken cancellationToken)
        {
            try
            {
                var length = data.Span[15];
                if (length > 0)
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
                                if ((ipui.Number >> 20) == 0x3014)
                                {
                                    Console.WriteLine(data.ToHex());
                                    var span = iedata.Span;
                                    ////00000080 b0100301 400fdf
                                    span[2] = 0x13;
                                    span[3] = 0x01;
                                    span[4] = 0x40;
                                    span[4] |= (byte) (ipui.Number >> 16 & 0xf);
                                    BinaryPrimitives.WriteUInt16BigEndian(span.Slice(5), (ushort) ipui.Number);
                                    span[7] = 0x00;
                                    Console.WriteLine($"shifted   {data.ToHex()}");
                                }
                            }
                            else
                            {
                                if ((ipui.Number >> 28) == 0x3014)
                                {
                                    Console.WriteLine(data.ToHex());
                                    //00000080 b0100301 400fdf
                                    var span = iedata.Span;
                                    span[2] = 0x10;
                                    span[3] = 0x03;
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
            }
            return WriteAsync(direction, messageId, rfp, data, cancellationToken);
        }
    }
}