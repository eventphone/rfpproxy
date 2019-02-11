using System;
using System.IO;
using System.Text;
using RfpProxy.Log.Messages;
using RfpProxyLib;
using Xunit;
using Xunit.Abstractions;

namespace RfpProxy.Test
{
    public class MessageTest
    {
        private readonly ITestOutputHelper _output;

        public MessageTest(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void CanDecodeRfpInitReq()
        {
            var data = HexEncoding.HexToByte(("03010023" +
                                             "7802" +
                                             "04 05 102af12c26" +
                                             "06 02 ca42" +
                                             "07 05 0010000000" +
                                             "26 02 0000" +
                                             "0d 03 1003ff" +
                                             "27 01 1f  28 01 00").Replace(" ", String.Empty));

            var message = AaMiDeMessage.Create(data);
            var rfpc = Assert.IsType<DnmRfpcMessage>(message);
            Assert.Equal(DnmRfpcType.InitReq, rfpc.DnmType);

            using (var writer = new StringWriter())
            {
                message.Log(writer);
                _output.WriteLine(writer.ToString());
            }
        }
    }
}