using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Mono.Options;

namespace RfpProxy
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string host = "localhost";
            int port = 16321;
            int listen = 16000;
            bool showHelp = false;
            bool useTProxy = false;
            var options = new OptionSet
            {
                {"H|host=", "hostname of OMM", x => host = x},
                {"p|port=", "port number of OMM", (ushort x) => port = x},
                {"l|listen=", "TCP listen port of proxy", (ushort x) => listen = x},
                {"t|transparent", "use TPROXY", x => useTProxy = x != null},
                {"h|help", "show help", x => showHelp = x != null},
            };

            try
            {
                if (options.Parse(args).Count > 0)
                {
                    showHelp = true;
                }
            }
            catch(OptionException ex)
            {
                Console.Error.Write("rfpproxy: ");
                Console.Error.WriteLine(ex.Message);
                Console.Error.WriteLine("Try 'dotnet rfpproxy.dll --help' for more information.");
                return;
            }

            if (showHelp)
            {
                options.WriteOptionDescriptions(Console.Error);
                return;
            }

            using (var cts = new CancellationTokenSource())
            using (var proxy = new RfpProxy(listen, host, port) {UseTProxy = useTProxy})
            {
                Console.CancelKeyPress += (s, e) =>
                {
                    e.Cancel = true;
                    cts.Cancel();
                };
                await proxy.RunAsync(cts.Token).ConfigureAwait(false);
            }
        }
    }
}
