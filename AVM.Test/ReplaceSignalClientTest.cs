using RfpProxy.AVM;
using RfpProxyLib;
using RfpProxyLib.Messages;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AVM.Test
{
    public class ReplaceSignalClientTest
    {
        [Fact]
        public void CanReplaceSignal()
        {
            using (var client = new TestClient())
            {
                var data = HexEncoding.HexToByte("0301002e 79050810 29132099 03050508 80b01002 9649cf19 0606a0a0 102af12c e080e440 6c064080 31303030 7c048002 0084".Replace(" ", String.Empty));
                client.TestAsync(data);
                Assert.Equal(0x41, data[35]);
            }
        }
    }

    public class TestClient : ReplaceSignalClient
    {
        public TestClient() : base(String.Empty)
        {
        }

        public Task TestAsync(Memory<byte> data)
        {
            return base.OnMessageAsync(MessageDirection.FromRfp, 0, default, data, CancellationToken.None);
        }

        public override Task WriteAsync(MessageDirection direction, uint messageId, RfpIdentifier rfp, ReadOnlyMemory<byte> data, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
