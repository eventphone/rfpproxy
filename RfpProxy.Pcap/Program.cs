using System;
using System.Buffers.Binary;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Mono.Options;
using RfpProxyLib;
using RfpProxyLib.Messages;

namespace RfpProxy.Pcap
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string socketname = "client.sock";
            string mac = "000000000000";
            string rfpMmask = "000000000000";
            string filter = "0301";
            string filterMask = "ffff";
            string filename = null;
            bool showHelp = false;
            var options = new OptionSet
            {
                {"s|socket=", "socket path", x => socketname = x},
                {"r|rfp=", "rfp MAC address", x => mac = x},
                {"rm|rfpmask=", "rfp mask", x=>rfpMmask = x},
                {"f|filename=", "pcap file path", x=>filename = x},
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
                Console.Error.Write("rfpproxy.pcap: ");
                Console.Error.WriteLine(ex.Message);
                Console.Error.WriteLine("Try 'dotnet rfpproxy.pcap.dll --help' for more information");
                return;
            }
            if (String.IsNullOrEmpty(filename))
                showHelp = true;
            if (showHelp)
            {
                options.WriteOptionDescriptions(Console.Error);
                return;
            }
            try
            {
                using (var cts = new CancellationTokenSource())
                using (var client = new PcapClient(socketname, filename))
                {
                    Console.CancelKeyPress += (s, e) =>
                    {
                        e.Cancel = true;
                        cts.Cancel();
                        client.Stop();
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
    }

    class PcapClient : ProxyClient
    {
        private readonly FileStream _file;

        public PcapClient(string socket, string filename) : base(socket)
        {
            _file = File.OpenWrite(filename);
            _file.SetLength(0);
            WritePcapHeader();
        }

        private void WritePcapHeader()
        {
            var header = new byte[24];
            header[0] = 0xa1; // magic
            header[1] = 0xb2;
            header[2] = 0xc3;
            header[3] = 0xd4;

            header[4] = 0x00; // major version
            header[5] = 0x02;
            header[6] = 0x00; // minor version
            header[7] = 0x04;
            
            header[16] = 0xff; // snaplen
            header[17] = 0xff;
            header[18] = 0xff;
            header[19] = 0xff;

            header[23] = 0x01; // ethernet
            _file.Write(header);
        }

        protected override async Task OnMessageAsync(MessageDirection direction, uint messageId, RfpIdentifier rfp, Memory<byte> data, CancellationToken cancellationToken)
        {
            data = data.Slice(4);
            var packetheaderlength = 6 + 6 + 2 + 4;
            var timestamp = DateTimeOffset.UtcNow;
            var header = new byte[16];
            BinaryPrimitives.WriteUInt32BigEndian(header, (uint) timestamp.ToUnixTimeSeconds());
            BinaryPrimitives.WriteUInt32BigEndian(header.AsSpan(4), (uint) timestamp.Millisecond * 1000);
            BinaryPrimitives.WriteUInt32BigEndian(header.AsSpan(8), (uint) (data.Length + packetheaderlength));
            BinaryPrimitives.WriteUInt32BigEndian(header.AsSpan(12), (uint) (data.Length + packetheaderlength));
            await _file.WriteAsync(header, cancellationToken).ConfigureAwait(false);

            header = new byte[packetheaderlength];
            rfp.CopyTo(header);
            rfp.CopyTo(header.AsSpan(6));
            if (direction == MessageDirection.FromOmm)
            {
                header[6] = 0x02;
            }
            else
            {
                header[0] = 0x02;
            }
            header[12] = 0xa0;

            BinaryPrimitives.WriteUInt16BigEndian(header.AsSpan(14), (ushort) data.Length);
            header[16] = 0xba;
            header[17] = 0xbe;
            await _file.WriteAsync(header, cancellationToken).ConfigureAwait(false);

            await _file.WriteAsync(data, cancellationToken).ConfigureAwait(false);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                _file.Dispose();
            }
        }
    }
}
