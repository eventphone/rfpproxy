using System;
using System.Buffers.Binary;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Schema;
using Mono.Options;
using RfpProxyLib;
using RfpProxyLib.AaMiDe.Media;

namespace SuperMarioBrothers
{
    class Program
    {
        static async Task Main(string[] args)
        {
            using (var c = new SmbClient("amelie", string.Empty))
            {
                var tones = c.GetTones().ToArray();
                var compressor = new ToneCompressor(tones, 256);
                tones = compressor.Compress();
                var total = 0;
                foreach (var tone in tones)
                {
                    tone.Log(Console.Out);
                    Console.WriteLine();
                    total++;
                }
                Console.WriteLine(total);
                var message = new MediaToneMessage(0x68F8, MediaDirection.TxRx, 0, tones);
                var data = new byte[message.Length];
                message.Serialize(data);
                var hex = data.AsMemory().ToHex();
                for (int i = 0; i < hex.Length; i++)
                {
                    Console.Write(hex[i]);
                    if (i % 2400 == 0 && i != 0)
                        Console.WriteLine();
                }
            }
            string socketname = "client.sock";
            bool showHelp = false;
            string mac = null;
            var options = new OptionSet
            {
                {"s|socket=", "socket path", x => socketname = x},
                {"r|rfp=", "rfp MAC address", x => mac = x},
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
                Console.Error.Write("rfpproxyinject: ");
                Console.Error.WriteLine(ex.Message);
                Console.Error.WriteLine("Try 'dotnet rfpproxyinject.dll --help' for more information");
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
                using (var client = new SmbClient("smb", socketname))
                {
                    Console.CancelKeyPress += (s, e) =>
                    {
                        e.Cancel = true;
                        cts.Cancel();
                        client.Stop();
                    };
                    await client.FinishHandshakeAsync(cts.Token);
                    var rfp = HexEncoding.HexToByte(mac);
                    Console.WriteLine("RTS. Enter a HDL per line to inject Super Mario Brothers. Empty HDL to close.");
                    do
                    {
                        var message = Console.ReadLine();
                        if (String.IsNullOrEmpty(message))
                            break;
                        var hdl = BinaryPrimitives.ReadUInt16BigEndian(HexEncoding.HexToByte(message));
                        
                        await client.InjectAudioAsync(hdl);
                    } while (true);
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
}
