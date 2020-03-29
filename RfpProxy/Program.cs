using System;
using System.IO;
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
            string socket = "client.sock";
            string configfile = "/opt/SIP-DECT/tmp/omm_conf.txt";
            bool showHelp = false;
            bool useTProxy = false;
            var options = new OptionSet
            {
                {"H|host=", "hostname of OMM", x => host = x},
                {"p|port=", "port number of OMM", (ushort x) => port = x},
                {"l|listen=", "TCP listen port of proxy", (ushort x) => listen = x},
                {"s|socket=", "socket path", (string x) => socket = x},
                {"t|transparent", "use TPROXY", x => useTProxy = x != null},
                {"c|config", "omm_conf.txt", x=> configfile = x },
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
            using (var proxy = new RfpProxy(listen, host, port, socket, configfile) {UseTProxy = useTProxy})
            {
                Console.CancelKeyPress += (s, e) =>
                {
                    e.Cancel = true;
                    cts.Cancel();
                };
                await proxy.RunAsync(cts.Token).ConfigureAwait(false);
                try
                {
                    File.Delete(socket);
                }
                catch
                {
                    //ignore
                }
            }
        }
    }
}
