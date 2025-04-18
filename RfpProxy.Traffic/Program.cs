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
            bool omm = false;
            var options = new OptionSet
            {
                {"s|socket=", "socket path", x => socketname = x},
                {"r|rfp=", "rfp MAC address", x => mac = x},
                {"o|omm", "generate traffic to omm", x=>omm = x != null},
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
                using (var client = new TrafficClient(omm, socketname))
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
                    if (omm)
                    {
                        await client.AddHandlerAsync(0, mac, "ffffffffffff", "00030008deadbeefbabefefe", "ffffffffffffffffffffffff", cts.Token);
                    }
                    else
                    {
                        await client.AddHandlerAsync(0, mac, "ffffffffffff", "010e001800000000000000000000ffff0ac000195000000000adbeef", "ffffffffffffffffffffffffffffffffffffffff00000000ffffffff", cts.Token);
                    }
                    await client.FinishHandshakeAsync(cts.Token);
                    if (omm)
                    {
                        await client.WriteAsync(MessageDirection.ToOmm, 0, rfp, HexEncoding.HexToByte("00030008deadbeefbabefefe"), cts.Token);
                    }
                    else
                    {
                        await client.WriteAsync(MessageDirection.ToRfp, 0, rfp, HexEncoding.HexToByte("010e001800000000000000000000ffff0ac0001950000000deadbeef"), cts.Token);
                    }
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
            private readonly bool _omm;
            private readonly byte[] _ping;

            public TrafficClient(bool omm, string socket) : base(socket)
            {
                _omm = omm;
                if (_omm)
                {
                    _ping = HexEncoding.HexToByte("00030008" +
                                                  "deadbeef" +
                                                  "babefefe");
                }
                else
                {
                    _ping = HexEncoding.HexToByte("010e0018" +
                                                  "00000000000000000000ffff0ac000195" +
                                                  "0000000" +
                                                  "deadbeef");
                }
            }

            protected override async Task OnMessageAsync(MessageDirection direction, uint messageId, RfpIdentifier rfp, Memory<byte> data, CancellationToken cancellationToken)
            {
                if (_omm && direction == MessageDirection.ToOmm)
                {
                    await WriteAsync(direction, messageId, rfp, data, cancellationToken).ConfigureAwait(false);
                    return;
                }
                
                await WriteAsync(direction, messageId, rfp, ReadOnlyMemory<byte>.Empty, cancellationToken);
                await WriteAsync(_omm ? MessageDirection.ToOmm : MessageDirection.ToRfp, 0, rfp, _ping, cancellationToken);
            }
        }
    }
}
