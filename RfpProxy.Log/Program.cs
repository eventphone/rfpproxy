using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Mono.Options;
using RfpProxyLib.Messages;
using RfpProxy.Log.Messages;
using RfpProxyLib;
using System.Buffers.Binary;
using RfpProxy.Log.Messages.Dnm;

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
            string pcap = null;
            var options = new OptionSet
            {
                {"s|socket=", "socket path", x => socketname = x},
                {"r|rfp=", "rfp MAC address", x => mac = x},
                {"rm|rfpmask=", "rfp mask", x => rfpMmask = x},
                {"f|filter=", "filter", x => filter = x},
                {"fm|filtermask=", "filter mask", x => filterMask = x},
                {"raw", "log raw packets", x => logRaw = x != null},
                {"u|unknown", "log only packets with unknown payload", x => unknown = x != null},
                {"pcap=", "read packets from pcap file", x=>pcap = x},
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
                    if (pcap != null)
                    {
                        await ReadPcapAsync(pcap, client.OnMessage, cts.Token).ConfigureAwait(false);
                        return;
                    }

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

        static async Task ReadPcapAsync(string file, Action<MessageDirection, RfpIdentifier, ReadOnlyMemory<byte>, DateTimeOffset> messageCallback, CancellationToken cancellationToken)
        {
            using (var s = File.OpenRead(file))
            {
                var pcapHeader = new byte[24];
                var success = await FillBufferAsync(s, pcapHeader, cancellationToken).ConfigureAwait(false);
                if (!success) return;
                if (BinaryPrimitives.ReadUInt32BigEndian(pcapHeader) != 0xa1b2c3d4 || pcapHeader[23] != 0x01)
                {
                    Console.WriteLine("Invalid pcap file");
                    return;
                }
                var packetHeader = new byte[16 + 54];
                while (true)
                {
                    success = await FillBufferAsync(s, packetHeader, cancellationToken).ConfigureAwait(false);
                    if (!success) return;
                    var direction = packetHeader[16] == 0x02 ? MessageDirection.ToOmm : MessageDirection.FromOmm;
                    RfpIdentifier rfp;
                    if (direction == MessageDirection.FromOmm)
                    {
                        rfp = new RfpIdentifier(packetHeader.AsMemory(16, 6));
                    }
                    else
                    {
                        rfp = new RfpIdentifier(packetHeader.AsMemory(16 + 6, 6));
                    }
                    var length = BinaryPrimitives.ReadUInt32BigEndian(packetHeader.AsSpan(12));
                    var data = new byte[length - 54];
                    success = await FillBufferAsync(s, data, cancellationToken).ConfigureAwait(false);
                    if (!success) return;
                    var seconds = BinaryPrimitives.ReadUInt32BigEndian(packetHeader);
                    var milliseconds = BinaryPrimitives.ReadUInt32BigEndian(packetHeader.AsSpan(4)) / 1000;
                    messageCallback(direction, rfp, data, DateTimeOffset.FromUnixTimeSeconds(seconds).AddMilliseconds(milliseconds));
                }
            }
        }
        
        private static async Task<bool> FillBufferAsync(FileStream file, Memory<byte> buffer, CancellationToken cancellationToken)
        {
            while (buffer.Length > 0)
            {
                var bytesRead = await file.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
                if (bytesRead == 0) return false;
                buffer = buffer.Slice(bytesRead);
            }
            return true;
        }

        class LogClient : ProxyClient
        {
            private readonly bool _logRaw;
            private readonly bool _unknown;
            private readonly Dictionary<RfpIdentifier, AaMiDeReassembler> _rfpReassemblers = new Dictionary<RfpIdentifier, AaMiDeReassembler>();
            private readonly Dictionary<RfpIdentifier, AaMiDeReassembler> _ommReassemblers = new Dictionary<RfpIdentifier, AaMiDeReassembler>();

            public LogClient(string socket, bool logRaw, bool unknown) : base(socket)
            {
                _logRaw = logRaw;
                _unknown = unknown;
            }

            protected override Task OnMessageAsync(MessageDirection direction, uint messageId, RfpIdentifier rfp, Memory<byte> data, CancellationToken cancellationToken)
            {
                OnMessage(direction, rfp, data, DateTimeOffset.Now);
                return Task.CompletedTask;
            }

            public void OnMessage(MessageDirection direction, RfpIdentifier rfp, ReadOnlyMemory<byte> data, DateTimeOffset timestamp)
            { 
                if (data.IsEmpty)
                    return;
                AaMiDeMessage message;
                string prefix;
                AaMiDeReassembler reassembler;
                if (direction == MessageDirection.FromOmm)
                {
                    if (!_ommReassemblers.TryGetValue(rfp, out reassembler))
                    {
                        reassembler = new AaMiDeReassembler();
                        _ommReassemblers.Add(rfp, reassembler);
                    }
                    prefix = "OMM:";
                }
                else
                {
                    if (!_rfpReassemblers.TryGetValue(rfp, out reassembler))
                    {
                        reassembler = new AaMiDeReassembler();
                        _rfpReassemblers.Add(rfp, reassembler);
                    }
                    prefix = "RFP:";
                }
                try
                {
                    message = AaMiDeMessage.Create(data, reassembler);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{timestamp:yyyy/MM/dd HH:mm:ss.fff} {prefix}{rfp} Cannot parse {data.ToHex()}");
                    Console.WriteLine(ex);
                    return;
                }
                if (message is DnmMessage dnm)
                {
                    if (dnm.Payload is MacDisIndPayload || dnm.DnmType == DnmType.MacDisReq)
                    {
                        bool clearReassembler;
                        if (direction == MessageDirection.FromOmm)
                        {
                            clearReassembler = _rfpReassemblers.TryGetValue(rfp, out reassembler);
                        }
                        else
                        {
                            clearReassembler = _ommReassemblers.TryGetValue(rfp, out reassembler);
                        }
                        if (clearReassembler)
                        {
                            var nwk = reassembler.GetNwk(dnm.MCEI);
                            nwk.Clear();
                            reassembler.Return(dnm.MCEI, nwk);
                        }
                    }
                }
                if (_unknown && !message.HasUnknown)
                    return;
                Console.Write($"{timestamp:yyyy/MM/dd HH:mm:ss.fff} {prefix}{rfp} ");
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
                return;
            }
        }
    }
}