using System;
using System.Buffers.Binary;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
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
            
            proxy.SendFromRfp("dd6d8018a5de60836d515a817fade8e8e8324e2fa96149dd938f209c9fe685655cf8960c" +
                              "e91604f4c7569b85430ed9000bcb900bfa65bacf86cfc2bcf080a649ab404d28c14c89c0" +
                              "44dc0b669817084e83349e02575f24213d8ecbb54c74dcdddf1594642bc7ec248ac7dfa0" +
                              "c3ec3a4b783f09d3c68fc5818c99b086164fa231f2c77e9a08e8c58b90336a802fba08f2" +
                              "e5bc07a0fee006e1535d82320ef78f0c48b7f02cddc173ea77d6c744226b42a570080208" +
                              "eb6cb2041aa7251631a4fa7e68ce32168be6dbb380e06e11be95c4e5852205e056d7f700" +
                              "9d30e415a9f12da68df6a4ea7c15308e53319466af552c95b8d3a24c2ad03936176daef0" +
                              "51e1733d9420707ce57e7ebaf4f34017e464695b779cf32c61eef8dad30b2076090c1a58" +
                              "068752705e0ce88648beaaaae2dcef95668c3b687116ed5b060eda7a2763e008585d9c6a" +
                              "a9ba40b1d3819c4874174ba77e5a850c3f7d6e4f9d0f674e9ca8a1d9ae7d162785759f2e" +
                              "136f29ff8b6285d5f2590ffb64ffad894caf331604f62d7aee3f697997f876be2a37bc6d" +
                              "cf9aae30da8d0fc52fb176199fedc4fee321d5f2");
            var msg = proxy.LastRfpMessage;
            ValidateTypeAndLength(msg);
            Assert.True(msg.Span.StartsWith(new byte[]{0x01,0x20,0x01,0x04}));
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

            public TestProxy() : base(51337, "localhost", 61337)
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
                get { return BlowFish.ByteToHex(LastRfpMessage.Span); }
            }

            public ReadOnlyMemory<byte> LastOmmMessage { get; private set; }

            public ManualResetEventSlim RfpMessageEvent
            {
                get { return _rfpMessage; }
            }

            public void SendFromOmm(string hex)
            {
                SendFromOmm(BlowFish.HexToByte(hex));
            }

            public void SendFromOmm(ReadOnlySpan<byte> data)
            {
                _ommMessage.Reset();
                _ommClient.Client.Send(data);
                _ommMessage.Wait();
            }

            public void SendFromRfp(string hex)
            {
                SendFromRfp(BlowFish.HexToByte(hex));
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