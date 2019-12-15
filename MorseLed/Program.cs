using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Mono.Options;
using RfpProxyLib;
using RfpProxyLib.Messages;

namespace RfpProxy.MorseLed
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
            List<string> message;
            try
            {
                message = options.Parse(args);
                if (message.Count < 1 || rfps.Count == 0)
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
                using (var client = new MorseLedClient(socketname, rfps))
                {
                    Console.CancelKeyPress += (s, e) =>
                    {
                        e.Cancel = true;
                        cts.Cancel();
                        client.Stop();
                    };
                    await client.MorseAsync(String.Join(' ', message), cts.Token);
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

    public class MorseLedClient : ProxyClient
    {
        private readonly RfpIdentifier[] _rfps;

        public MorseLedClient(string socket, ICollection<string> rfps) : base(socket)
        {
            _rfps = rfps.Select(x=>new RfpIdentifier(HexEncoding.HexToByte(x))).ToArray();
        }

        protected override Task OnMessageAsync(MessageDirection direction, uint messageId, RfpIdentifier rfp, Memory<byte> data, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        private static readonly byte[] _on = HexEncoding.HexToByte("0102000403010000");
        private static readonly byte[] _off = HexEncoding.HexToByte("0102000403000000");
        private static readonly int _duration = 1000;

        public async Task MorseAsync(string message, CancellationToken cancellationToken)
        {
            await FinishHandshakeAsync(cancellationToken);
            var pattern = ConvertToMorse(message);
            while (!cancellationToken.IsCancellationRequested)
            {
                foreach (var code in pattern)
                {
                    Console.WriteLine(code);
                    if (code == Morse.None)
                    {
                        await Task.Delay(_duration, cancellationToken);
                    }
                    else
                    {
                        foreach (var rfp in _rfps)
                        {
                            await WriteAsync(MessageDirection.ToRfp, 0, rfp, _on, cancellationToken);
                        }

                        if (code == Morse.Dah)
                            await Task.Delay(3 * _duration, cancellationToken);
                        else 
                            await Task.Delay(_duration, cancellationToken);

                        foreach (var rfp in _rfps)
                        {
                            await WriteAsync(MessageDirection.ToRfp, 0, rfp, _off, cancellationToken);
                        }
                    }
                    await Task.Delay(_duration, cancellationToken);
                }
            }
            cancellationToken.ThrowIfCancellationRequested();
        }

        private ICollection<Morse> ConvertToMorse(string message)
        {
            var result = new List<Morse>();
            foreach (var c in message.ToLowerInvariant())
            {
                switch (c)
                {
                    case ' ':
                        result.Add(Morse.None);
                        result.Add(Morse.None);
                        break;
                    case 'a':
                        result.Add(Morse.Dit);
                        result.Add(Morse.Dah);
                        break;
                    case 'b':
                        result.Add(Morse.Dah);
                        result.Add(Morse.Dit);
                        result.Add(Morse.Dit);
                        result.Add(Morse.Dit);
                        break;
                    case 'c':
                        result.Add(Morse.Dah);
                        result.Add(Morse.Dit);
                        result.Add(Morse.Dah);
                        result.Add(Morse.Dit);
                        break;
                    case 'd':
                        result.Add(Morse.Dah);
                        result.Add(Morse.Dit);
                        result.Add(Morse.Dit);
                        break;
                    case 'e':
                        result.Add(Morse.Dit);
                        break;
                    case 'f':
                        result.Add(Morse.Dit);
                        result.Add(Morse.Dit);
                        result.Add(Morse.Dah);
                        result.Add(Morse.Dit);
                        break;
                    case 'g':
                        result.Add(Morse.Dah);
                        result.Add(Morse.Dah);
                        result.Add(Morse.Dit);
                        break;
                    case 'h':
                        result.Add(Morse.Dit);
                        result.Add(Morse.Dit);
                        result.Add(Morse.Dit);
                        result.Add(Morse.Dit);
                        break;
                    case 'i':
                        result.Add(Morse.Dit);
                        result.Add(Morse.Dit);
                        break;
                    case 'j':
                        result.Add(Morse.Dit);
                        result.Add(Morse.Dah);
                        result.Add(Morse.Dah);
                        result.Add(Morse.Dah);
                        break;
                    case 'k':
                        result.Add(Morse.Dah);
                        result.Add(Morse.Dit);
                        result.Add(Morse.Dah);
                        break;
                    case 'l':
                        result.Add(Morse.Dit);
                        result.Add(Morse.Dah);
                        result.Add(Morse.Dit);
                        result.Add(Morse.Dit);
                        break;
                    case 'm':
                        result.Add(Morse.Dah);
                        result.Add(Morse.Dah);
                        break;
                    case 'n':
                        result.Add(Morse.Dah);
                        result.Add(Morse.Dit);
                        break;
                    case 'o':
                        result.Add(Morse.Dah);
                        result.Add(Morse.Dah);
                        result.Add(Morse.Dah);
                        break;
                    case 'p':
                        result.Add(Morse.Dit);
                        result.Add(Morse.Dah);
                        result.Add(Morse.Dah);
                        result.Add(Morse.Dit);
                        break;
                    case 'q':
                        result.Add(Morse.Dah);
                        result.Add(Morse.Dah);
                        result.Add(Morse.Dit);
                        result.Add(Morse.Dah);
                        break;
                    case 'r':
                        result.Add(Morse.Dit);
                        result.Add(Morse.Dah);
                        result.Add(Morse.Dit);
                        break;
                    case 's':
                        result.Add(Morse.Dit);
                        result.Add(Morse.Dit);
                        result.Add(Morse.Dit); 
                        break;
                    case 't':
                        result.Add(Morse.Dah);
                        break;
                    case 'u':
                        result.Add(Morse.Dit);
                        result.Add(Morse.Dit);
                        result.Add(Morse.Dah);
                        break;
                    case 'v':
                        result.Add(Morse.Dit);
                        result.Add(Morse.Dit);
                        result.Add(Morse.Dit);
                        result.Add(Morse.Dah);
                        break;
                    case 'w':
                        result.Add(Morse.Dit);
                        result.Add(Morse.Dah);
                        result.Add(Morse.Dah);
                        break;
                    case 'x':
                        result.Add(Morse.Dah);
                        result.Add(Morse.Dit);
                        result.Add(Morse.Dit);
                        result.Add(Morse.Dah);
                        break;
                    case 'y':
                        result.Add(Morse.Dah);
                        result.Add(Morse.Dit);
                        result.Add(Morse.Dah);
                        result.Add(Morse.Dah);
                        break;
                    case 'z':
                        result.Add(Morse.Dah);
                        result.Add(Morse.Dah);
                        result.Add(Morse.Dit);
                        result.Add(Morse.Dit);
                        break;
                    case '0':
                        result.Add(Morse.Dah);
                        result.Add(Morse.Dah);
                        result.Add(Morse.Dah);
                        result.Add(Morse.Dah);
                        result.Add(Morse.Dah);
                        break;
                    case '1':
                        result.Add(Morse.Dit);
                        result.Add(Morse.Dah);
                        result.Add(Morse.Dah);
                        result.Add(Morse.Dah);
                        result.Add(Morse.Dah);
                        break;
                    case '2':
                        result.Add(Morse.Dit);
                        result.Add(Morse.Dit);
                        result.Add(Morse.Dah);
                        result.Add(Morse.Dah);
                        result.Add(Morse.Dah);
                        break;
                    case '3':
                        result.Add(Morse.Dit);
                        result.Add(Morse.Dit);
                        result.Add(Morse.Dit);
                        result.Add(Morse.Dah);
                        result.Add(Morse.Dah);
                        break;
                    case '4':
                        result.Add(Morse.Dit);
                        result.Add(Morse.Dit);
                        result.Add(Morse.Dit);
                        result.Add(Morse.Dit);
                        result.Add(Morse.Dah);
                        break;
                    case '5':
                        result.Add(Morse.Dit);
                        result.Add(Morse.Dit);
                        result.Add(Morse.Dit);
                        result.Add(Morse.Dit);
                        result.Add(Morse.Dit);
                        break;
                    case '6':
                        result.Add(Morse.Dah);
                        result.Add(Morse.Dit);
                        result.Add(Morse.Dit);
                        result.Add(Morse.Dit);
                        result.Add(Morse.Dit);
                        break;
                    case '7':
                        result.Add(Morse.Dah);
                        result.Add(Morse.Dah);
                        result.Add(Morse.Dit);
                        result.Add(Morse.Dit);
                        result.Add(Morse.Dit);
                        break;
                    case '8':
                        result.Add(Morse.Dah);
                        result.Add(Morse.Dah);
                        result.Add(Morse.Dah);
                        result.Add(Morse.Dit);
                        result.Add(Morse.Dit);
                        break;
                    case '9':
                        result.Add(Morse.Dah);
                        result.Add(Morse.Dah);
                        result.Add(Morse.Dah);
                        result.Add(Morse.Dah);
                        result.Add(Morse.Dit);
                        break;
                }
                result.Add(Morse.None);
            }
            result.Add(Morse.None);
            result.Add(Morse.None);
            return result;
        }

        public enum Morse
        {
            None,
            Dit,
            Dah,
        }
    }
}
