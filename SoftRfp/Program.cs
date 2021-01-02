using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Mono.Options;

namespace RfpProxy.Virtual
{
    class Program
    {
        static async Task Main(string[] args)
        {
            bool showHelp = false;
            string mac = null;
            string omm = null;
            string rfpa = null;
            var options = new OptionSet
            {
                {"m|mac=", "rfp MAC address", x => mac = x},
                {"o|omm=", "OMM ip address", x => omm = x},
                {"k|key=", "RFPA (blowfish key)", x => rfpa = x},
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
                Console.Error.Write("virtualrfp: ");
                Console.Error.WriteLine(ex.Message);
                Console.Error.WriteLine("Try 'dotnet virtualrfp.dll --help' for more information");
                return;
            }
            if (String.IsNullOrEmpty(mac) || String.IsNullOrEmpty(omm))
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
                {
                    Console.CancelKeyPress += (s, e) =>
                    {
                        e.Cancel = true;
                        cts.Cancel();
                    };
                    var client = new VirtualRfp(mac, omm, rfpa);
                    client.OnMessage += (s, e) =>
                    {
                        e.Message.Log(Console.Out);
                        Console.WriteLine();
                    };
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
}
