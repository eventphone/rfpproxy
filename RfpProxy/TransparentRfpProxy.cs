using System;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace RfpProxy
{
    public abstract class TransparentRfpProxy : AbstractRfpProxy
    {
        private bool _useTProxy;

        protected TransparentRfpProxy(int listenPort, string ommHost, int ommPort)
            : base(listenPort, ommHost, ommPort)
        {
        }

        public bool UseTProxy
        {
            get => _useTProxy;
            set
            {
                if (value)
                {
                    if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                        throw new PlatformNotSupportedException("TPROXY requires linux kernel support");
                }
                _useTProxy = value;
            }
        }

        protected override TcpListener CreateListener(int port)
        {
            var listener = base.CreateListener(port);
            if (UseTProxy)
            {
                byte on = 1;
                var socket = listener.Server;
                if (NativeMethods.setsockopt(socket.Handle, SocketOptionLevel.IP, NativeMethods.IP_TRANSPARENT, ref on, 1) != 0)
                {
                    throw new PlatformNotSupportedException("error setting IP_TRANSPARENT");
                }
            }
            return listener;
        }

        protected override TcpClient ConnectToServer(TcpClient client)
        {
            TcpClient server;
            if (UseTProxy)
            {
                var socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);

                socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                socket.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, false);

                byte on = 1;

                if (NativeMethods.setsockopt(socket.Handle, SocketOptionLevel.IP, NativeMethods.IP_TRANSPARENT, ref on, 1) != 0)
                {
                    throw new PlatformNotSupportedException("error setting IP_TRANSPARENT");
                }

                socket.Bind(client.Client.RemoteEndPoint);
                server = new TcpClient {Client = socket};
            }
            else
            {
                server = base.ConnectToServer(client);
            }
            return server;
        }

        private class NativeMethods
        {
            public const int IP_TRANSPARENT = 19;

            [DllImport("libc.so.6")]
            public static extern int setsockopt(IntPtr socket, SocketOptionLevel level, int option_name, ref byte option_value, int option_len);
        }
    }
}