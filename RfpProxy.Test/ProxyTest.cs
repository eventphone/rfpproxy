using System;
using System.Buffers.Binary;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RfpProxyLib;
using Xunit;

namespace RfpProxy.Test
{
    public class ProxyTest
    {
        [Fact]
        public void CanHandshakeExistingRFP()
        {
            var proxy = new TestProxy("4685E31451732829A7C1211C21F3A6FAE39835770D27E37374BCFE786B72D30A50A12C6E5B65D55B8654FFFAEFC726F725E0F9233B626DE0AE8285DDB618D4F5");
            proxy.SendFromOmm("012d0020a40a896af9618b5bd0d56b3b6d321e98531e2a752008c5e6d756f5cc8eb4df8c");
            ValidateTypeAndLength(proxy.LastOmmMessage);

            proxy.SendFromRfp("012001100000000d000801000030421b17370000000000000001ef3c2a0d8ce8" +
                              "725371d0d799a0298dc02c734ac5b803abc38663b494de7b2ffbe03d70b616eb" +
                              "facf2e7d85f61b295cba5c76ea515501b3c02b755862261bfc08ffde00080201" +
                              "00000000000000000000000000000000000000005349502d4445435420382e31" +
                              "5350312d46413237000000000000000000000000000000000000000000000000" +
                              "0000000000000000000000000000000000000000000000000000000000000000" +
                              "0000000000000000000000000000000000000000000000000000000000000000" +
                              "0000000000000000000000000000000000000000000000000000000000000000" +
                              "0000000072f90d3b1771c77a7acbfa0a8e7ad3cf");
            ValidateTypeAndLength(proxy.LastRfpMessage);
            
            proxy.SendFromOmm("000100080120ffff01000000");
            ValidateTypeAndLength(proxy.LastOmmMessage);

            proxy.SendFromOmm("52df58b8ae32f14a");
            ValidateTypeAndLength(proxy.LastOmmMessage);
            Assert.Equal("010c000400000000", proxy.LastOmmMessageHex);
            proxy.SendFromOmm("05b54aca8fa7462d47a00b6027c816d2");
            ValidateTypeAndLength(proxy.LastOmmMessage);
            Assert.Equal("01010008b8b8200606000000", proxy.LastOmmMessageHex);
            proxy.SendFromOmm("dad130112199e1be");
            ValidateTypeAndLength(proxy.LastOmmMessage);
            Assert.Equal("010500040f000000", proxy.LastOmmMessageHex);
            proxy.SendFromOmm("e35dea95cd3a13dd");
            ValidateTypeAndLength(proxy.LastOmmMessage);
            Assert.Equal("0123000401000000", proxy.LastOmmMessageHex);
            proxy.SendFromOmm("f9fde7263e275143");
            ValidateTypeAndLength(proxy.LastOmmMessage);
            Assert.Equal("0102000401010000", proxy.LastOmmMessageHex);
            proxy.SendFromOmm("b30ece1f8c9731c2");
            ValidateTypeAndLength(proxy.LastOmmMessage);
            Assert.Equal("0102000400090000", proxy.LastOmmMessageHex);

            proxy.SendFromOmm("9d8fb229f310b7e994ba405be02d01f5ad4827d383925564" + 
                              "925f9fadc6e023f5db5f3f74cd91d17d8814a9946e7abdbe729b59d32d0b4725" +
                              "2b99c39b70fd362802862e0541b876dd4fe8e9291add80e03cf40c5355bfc168" +
                              "e041229eef995b6a74ba52de3ca84dce31510178296fc2fe86325b4663142052" +
                              "fe9c27b34f887a3ab2a9f3222cc8aefb8134c77edfa2b142c39da25a7ea2d8cc" +
                              "1fe10d67c2546499bb41e3645f7b6616a80b51d700b4ed17315298e13b0abf56" +
                              "b02ef601622f4837adde01b885dd370db331318dc1821a660b33e48ec3849542" +
                              "4675da6b111765dca00a7a1a3d1ae0b7a9e65088549347a81e77f512c6b4715b" +
                              "8ee78ca5cba5a5666a37472de28add2e96747ab669febdf4ab3c5b3ef24a7bb7" +
                              "1b3b0e7cb8e29f4926a9aba0a7467e9cb9ac61573b8deb6d6d6518d466256132" +
                              "ba122d69e163adb2997fda7c7b3cd8e2519f62624f38e5bc0604f197b5630337" +
                              "10657db308b8753744e2740a1125f79ea8c934bb34b7a3685a17c75f4221982e" +
                              "e6bc96ec466649fb4669c9afda51af4752a84287ff83c08e");
            ValidateTypeAndLength(proxy.LastOmmMessage);
            proxy.SendFromRfp("4e2da25833e64f92");
            ValidateTypeAndLength(proxy.LastRfpMessage);
            proxy.SendFromRfp("3c54c821a4e5961f32933ca3731667960dfb967332f26106");
            ValidateTypeAndLength(proxy.LastRfpMessage);
        }

        [Fact]
        public void CanHandshakeNewRFP()
        {
            var proxy = new TestProxy(null);

            proxy.SendFromOmm("012d0020a989ad248bc2202225f3e8cb98e1d1ace70eca7514c55995e6c455def71712e7");
            ValidateTypeAndLength(proxy.LastOmmMessage);

            proxy.SendFromRfp("012001100000000d000801000030421b17370000000000000001ef3c2a0d8ce8" +
                              "725371d0d799a0298dc02c734ac5b803abc38663b494de7b2ffbe03d70b616eb" +
                              "facf2e7d85f61b295cba5c76ea515501b3c02b755862261bfc08ffde00080201" +
                              "00000000000000000000000000000000000000005349502d4445435420382e31" +
                              "5350312d46413237000000000000000000000000000000000000000000000000" +
                              "0000000000000000000000000000000000000000000000000000000000000000" +
                              "0000000000000000000000000000000000000000000000000000000000000000" +
                              "0000000000000000000000000000000000000000000000000000000000000000" +
                              "000000004970a0287fa3c7ba7bd60b29aca4d18e");
            ValidateTypeAndLength(proxy.LastRfpMessage);

            proxy.SendFromOmm("012400408cea08411f48f903d99a0aab239f54ed00d9f03659d06e7a792096d0" +
                              "e50ab8391a0e7ef66b535139508fcac09f7fd57046def34acd40dc25912210f6" +
                              "d840fb46");
            ValidateTypeAndLength(proxy.LastOmmMessage);

            proxy.SendFromOmm("000100080120ffff01000000");
            ValidateTypeAndLength(proxy.LastOmmMessage);

            proxy.SendFromOmm("dea69960f2275c97");
            ValidateTypeAndLength(proxy.LastOmmMessage);
            Assert.Equal("010c000400000000", proxy.LastOmmMessageHex);
            proxy.SendFromOmm("49fdbe2e347e6d7e46c42b62ebb1f185");
            ValidateTypeAndLength(proxy.LastOmmMessage);
            Assert.Equal("01010008b8b8200606000000", proxy.LastOmmMessageHex);
            proxy.SendFromOmm("eb58acd18e8234d4");
            ValidateTypeAndLength(proxy.LastOmmMessage);
            Assert.Equal("010500040f000000", proxy.LastOmmMessageHex);
            proxy.SendFromOmm("17d72dee7349246f");
            ValidateTypeAndLength(proxy.LastOmmMessage);
            Assert.Equal("0123000401000000", proxy.LastOmmMessageHex);
            proxy.SendFromOmm("ed99963e8adfebf5");
            ValidateTypeAndLength(proxy.LastOmmMessage);
            Assert.Equal("0102000401030000", proxy.LastOmmMessageHex);
            proxy.SendFromOmm("127f707444180fe3");
            ValidateTypeAndLength(proxy.LastOmmMessage);
            Assert.Equal("0102000400090000", proxy.LastOmmMessageHex);
        }

        [Fact]
        public void CanHandshakeReEnrollment()
        {
            var proxy = new TestProxy(null, "$1$$juPq1oleiGg7WHdZ5itlC/");

            proxy.SendFromOmm("012d0020b2a666452b48119751289d4e55757c5c9ff740b51e359439f56d538c709afff1");
            ValidateTypeAndLength(proxy.LastOmmMessage);

            proxy.SendFromRfp("012001100000000d000801000030421b17370000000000000001ef3c2a0d8ce8" +
                              "725371d0d799a0298dc02c734ac5b803abc38663b494de7b2ffbe03d70b616eb" +
                              "facf2e7d85f61b295cba5c76ea515501b3c02b755862261bfc08ffde00080201" +
                              "00000000000000000000000000000000000000005349502d4445435420382e31" +
                              "5350312d46413237000000000000000000000000000000000000000000000000" +
                              "0000000000000000000000000000000000000000000000000000000000000000" +
                              "0000000000000000000000000000000000000000000000000000000000000000" +
                              "0000000000000000000000000000000000000000000000000000000000000000" +
                              "00000000871bd9da8e32fa161163b9d7ee6036cc");
            ValidateTypeAndLength(proxy.LastRfpMessage);

            proxy.SendFromOmm("012500602dd71d86a4f12f4289be371972c69801faa190ce3e5963ec542d9e4e" +
                              "df04d8a19e805f01e425a7aaf5737eebbb400117af19caf194ca4e5cc97cbaae" +
                              "8da835e5ac4455a18eba33fe238cc7956182b82985a50332b5fca198469e239c" +
                              "4e1b3cc7");
            ValidateTypeAndLength(proxy.LastOmmMessage);

            proxy.SendFromOmm("000100080120ffff01000000");
            ValidateTypeAndLength(proxy.LastOmmMessage);

            proxy.SendFromOmm("40e5725f1c0798d4");
            ValidateTypeAndLength(proxy.LastOmmMessage);
            Assert.Equal("010c000400000000", proxy.LastOmmMessageHex);
            proxy.SendFromOmm("16fc8d39daddc1b50d94384c7a8bed59");
            ValidateTypeAndLength(proxy.LastOmmMessage);
            Assert.Equal("01010008b8b8200606000000", proxy.LastOmmMessageHex);
            proxy.SendFromOmm("ca24bc7120b96729");
            ValidateTypeAndLength(proxy.LastOmmMessage);
            Assert.Equal("010500040f000000", proxy.LastOmmMessageHex);
            proxy.SendFromOmm("f7bec665e9e3c843");
            ValidateTypeAndLength(proxy.LastOmmMessage);
            Assert.Equal("0123000401000000", proxy.LastOmmMessageHex);
            proxy.SendFromOmm("882a19b3430bb9e6");
            ValidateTypeAndLength(proxy.LastOmmMessage);
            Assert.Equal("0102000401030000", proxy.LastOmmMessageHex);
            proxy.SendFromOmm("85b0e21633d9eb0a");
            ValidateTypeAndLength(proxy.LastOmmMessage);
            Assert.Equal("0102000400090000", proxy.LastOmmMessageHex);

        }

        [Fact]
        public void CanRekey()
        {
            var proxy = new TestProxy(null);
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
            private readonly string _rfpa;
            private readonly string _ommPw;
            private TcpListener _omm;
            private readonly TcpClient _ommClient;
            private readonly TcpClient _rfpClient;
            private readonly ManualResetEventSlim _rfpMessage;
            private readonly ManualResetEventSlim _ommMessage;

            public TestProxy(string rfpa):this(rfpa, null)
            {
            }

            public TestProxy(string rfpa, string ommPw) : base(51337, "localhost", 61337, "asdf", null)
            {
                _rfpa = rfpa;
                _ommPw = ommPw;
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
                get { return LastRfpMessage.ToHex(); }
            }

            public ReadOnlyMemory<byte> LastOmmMessage { get; private set; }

            public string LastOmmMessageHex
            {
                get { return LastOmmMessage.ToHex(); }
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

            protected override async Task OnClientMessageAsync(RfpConnection connection, ReadOnlyMemory<byte> data, CancellationToken cancellationToken)
            {
                await base.OnClientMessageAsync(connection, data, cancellationToken);
                LastRfpMessage = data;
                _rfpMessage.Set();
            }

            protected override async Task OnServerMessageAsync(RfpConnection connection, ReadOnlyMemory<byte> data, CancellationToken cancellationToken)
            {
                await base.OnServerMessageAsync(connection, data, cancellationToken);
                LastOmmMessage = data;
                _ommMessage.Set();
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

            protected override Task<Memory<byte>> GetRfpaAsync(CryptedRfpConnection connection, CancellationToken cancellationToken)
            {
                if (!(_rfpa is null))
                {
                    return Task.FromResult<Memory<byte>>(HexEncoding.HexToByte(_rfpa));
                }
                return Task.FromResult(Memory<byte>.Empty);
            }

            protected override Task<string> GetRootPasswordHashAsync(CancellationToken cancellationToken)
            {
                return Task.FromResult(_ommPw);
            }
        }
    }
}