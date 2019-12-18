using System;
using RfpProxyLib;
using RfpProxy.AaMiDe.Media;
using Xunit;

namespace MediaTone.Test
{
    public class SerializeTest
    {
        [Fact]
        public void CanSerializeMediaTone2Off()
        {
            var message = new MediaToneMessage(0xbabe, MediaDirection.TxRx, 0, Array.Empty<MediaToneMessage.Tone>());
            Assert.Equal(12, message.Length);
            var data = new byte[12];
            message.Serialize(data);
            Assert.Equal(2, data[0]);
            Assert.Equal(0xb, data[1]);
            Assert.Equal(0, data[2]);
            Assert.Equal(8, data[3]);
            Assert.Equal(0xbe, data[4]);
            Assert.Equal(0xba, data[5]);
            Assert.Equal(3, data[6]);
            Assert.True(data.AsSpan(7).IsEmpty());
        }
    }
}