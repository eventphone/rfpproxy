using System;
using System.Buffers.Binary;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using RfpProxyLib;
using Xunit;

namespace RfpProxy.Test
{
    public class ProxyTest
    {
        [Fact]
        public void CanHandshake()
        {
            var proxy = new TestProxy();
            proxy.SendFromOmm("012d00209bc21bab186210cd8d619849d192e75f28e79d0744696a53ba172a916a48d877");
            ValidateTypeAndLength(proxy.LastOmmMessage);

            proxy.SendFromRfp("01200104000000040007010000304212ebe2000000000000000003dce046da34" +
                              "17566137f9d6760257c68b2fadf109e904128438319bb0bffc0cee6b28e7fd28" +
                              "6bd92586861a59a9323b16491b95fc2ec42fdfb41dafcca7b0f2464600070100" +
                              "00000000000000005349502d4445435420372e312d434b313400000000000000" +
                              "0000000000000000000000000000000000000000000000000000000000000000" +
                              "0000000000000000000000000000000000000000000000000000000000000000" +
                              "0000000000000000000000000000000000000000000000000000000000000000" +
                              "00000000000000000000000000000000000000000000000042f9c74578cf9a17" +
                              "2fcc638d5761802d");
            ValidateTypeAndLength(proxy.LastRfpMessage);
            
            proxy.SendFromOmm("000100080120ffff01000000");
            ValidateTypeAndLength(proxy.LastOmmMessage);

            proxy.SendFromOmm("dd6d8018a5de6083");
            ValidateTypeAndLength(proxy.LastOmmMessage);
            proxy.SendFromOmm("6d515a817fade8e8e8324e2fa96149dd");
            ValidateTypeAndLength(proxy.LastOmmMessage);
            proxy.SendFromOmm("938f209c9fe68565");
            ValidateTypeAndLength(proxy.LastOmmMessage);
            proxy.SendFromOmm("5cf8960ce91604f4");
            ValidateTypeAndLength(proxy.LastOmmMessage);
            proxy.SendFromOmm("c7569b85430ed900");
            ValidateTypeAndLength(proxy.LastOmmMessage);
            proxy.SendFromOmm("0bcb900bfa65bacf");
            ValidateTypeAndLength(proxy.LastOmmMessage);
            Assert.Equal("0102000408003444", proxy.LastOmmMessageHex);
        }

        [Fact]
        public void CanRekey()
        {
            var proxy = new TestProxy();
            proxy.SendFromOmm("012d00209bc21bab186210cd8d619849d192e75f28e79d0744696a53ba172a916a48d877");
            proxy.SendFromRfp("01200104000000040007010000304212ebe2000000000000000003dce046da34" +
                              "17566137f9d6760257c68b2fadf109e904128438319bb0bffc0cee6b28e7fd28" +
                              "6bd92586861a59a9323b16491b95fc2ec42fdfb41dafcca7b0f2464600070100" +
                              "00000000000000005349502d4445435420372e312d434b313400000000000000" +
                              "0000000000000000000000000000000000000000000000000000000000000000" +
                              "0000000000000000000000000000000000000000000000000000000000000000" +
                              "0000000000000000000000000000000000000000000000000000000000000000" +
                              "00000000000000000000000000000000000000000000000042f9c74578cf9a17" +
                              "2fcc638d5761802d");
            proxy.SendFromOmm("000100080120ffff01000000");
            var data = File.ReadAllBytes("rekey.bin").AsSpan(0x108);
            proxy.SendFromRfp(data.Slice(0, data.Length -16));
            var resetEvent = proxy.RfpMessageEvent;
            while (resetEvent.Wait(TimeSpan.FromSeconds(1)))
            {
                resetEvent.Reset();
            }
            proxy.SendFromRfp(data.Slice(data.Length - 16));
            Assert.Equal("00030008f73bde0264190000", proxy.LastRfpMessageHex);
        }

        private void ValidateTypeAndLength(ReadOnlyMemory<byte> data)
        {
            Assert.True(data.Span[0] <= 0x08);
            var length = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(2).Span);
            Assert.Equal(length + 4, data.Length);
        }

        public class TestProxy:RfpProxy
        {
            private TcpListener _omm;
            private readonly TcpClient _ommClient;
            private readonly TcpClient _rfpClient;
            private readonly ManualResetEventSlim _rfpMessage;
            private readonly ManualResetEventSlim _ommMessage;

            public TestProxy() : base(51337, "localhost", 61337, "asdf")
            {
                _omm = TcpListener.Create(61337);
                _omm.Start();
                _rfpClient = new TcpClient();
                _ = RunAsync(CancellationToken.None);
                _rfpClient.Connect("localhost", 51337);
                _ommClient = _omm.AcceptTcpClient();
                _rfpMessage = new ManualResetEventSlim(false);
                _ommMessage = new ManualResetEventSlim(false);
            }

            public ReadOnlyMemory<byte> LastRfpMessage { get; private set; }

            public string LastRfpMessageHex
            {
                get { return HexEncoding.ByteToHex(LastRfpMessage.Span); }
            }

            public ReadOnlyMemory<byte> LastOmmMessage { get; private set; }

            public string LastOmmMessageHex
            {
                get { return HexEncoding.ByteToHex(LastOmmMessage.Span); }
            }

            public ManualResetEventSlim RfpMessageEvent
            {
                get { return _rfpMessage; }
            }

            public void SendFromOmm(string hex)
            {
                SendFromOmm(HexEncoding.HexToByte(hex));
            }

            public void SendFromOmm(ReadOnlySpan<byte> data)
            {
                _ommMessage.Reset();
                _ommClient.Client.Send(data);
                _ommMessage.Wait();
            }

            public void SendFromRfp(string hex)
            {
                SendFromRfp(HexEncoding.HexToByte(hex));
            }

            public void SendFromRfp(ReadOnlySpan<byte> data)
            {
                _rfpMessage.Reset();
                _rfpClient.Client.Send(data);
                _rfpMessage.Wait();
            }

            protected override Task OnClientMessageAsync(RfpConnection connection, ReadOnlyMemory<byte> data, CancellationToken cancellationToken)
            {
                LastRfpMessage = data;
                _rfpMessage.Set();
                return base.OnClientMessageAsync(connection, data, cancellationToken);
            }

            protected override Task OnServerMessageAsync(RfpConnection connection, ReadOnlyMemory<byte> data, CancellationToken cancellationToken)
            {
                LastOmmMessage = data;
                _ommMessage.Set();
                return base.OnServerMessageAsync(connection, data, cancellationToken);
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    _omm.Stop();
                    _ommClient.Dispose();
                    _rfpClient.Dispose();
                }
                base.Dispose(disposing);
            }
        }
    }
}