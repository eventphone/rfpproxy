﻿using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Mono.Options;

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
            bool dnm = false;
            bool showHelp = false;
            var options = new OptionSet
            {
                {"s|socket=", "socket path", x => socketname = x},
                {"r|rfp=", "rfp MAC address", x => mac = x},
                {"rm|rfpmask=", "rfp mask", x => rfpMmask = x},
                {"f|filename=", "pcap file path - use '-' for STDOUT", x => filename = x},
                {"dnm", "use internal rfp ether protocol", x => dnm = x != null},
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
                Console.Error.Write("rfpproxydump: ");
                Console.Error.WriteLine(ex.Message);
                Console.Error.WriteLine("Try 'rfpproxydump --help' for more information");
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
                var target = filename == "-" ? Console.OpenStandardOutput() : File.OpenWrite(filename!);
                PcapClient client;
                if (dnm)
                {
                    client = new DnmPcapClient(socketname, target);
                }
                else
                {
                    filterMask = "0000";
                    client = new AaMiDePcapClient(socketname, target);
                }
                using (var cts = new CancellationTokenSource())
                using (client)
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
}
