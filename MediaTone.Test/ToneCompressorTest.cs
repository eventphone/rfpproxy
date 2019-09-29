using System;
using System.IO;
using System.Linq;
using RfpProxyLib.AaMiDe.Media;
using SuperMarioBrothers;
using Xunit;
using Xunit.Abstractions;

namespace MediaTone.Test
{
    public class ToneCompressorTest 
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public ToneCompressorTest(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Theory]
        [InlineData("amelie")]
        [InlineData("portal")]
        [InlineData("smb")]
        public void CanCompress(string name)
        {
            using (var c = new SmbClient(name, string.Empty))
            {
                var tones = c.GetTones().ToArray();
                
                var compressor = new ToneCompressor(tones);
                var compressed = compressor.Compress();
                _testOutputHelper.WriteLine(compressed.Length.ToString());
                for (var i = 0; i < tones.Length; i++)
                {
                    var tone = tones[i];
                    using (var writer = new StringWriter())
                    {
                        tone.Log(writer);
                        _testOutputHelper.WriteLine(i + " " + writer);
                    }
                }
                for (var i = 0; i < compressed.Length; i++)
                {
                    var tone = compressed[i];
                    using (var writer = new StringWriter())
                    {
                        tone.Log(writer);
                        _testOutputHelper.WriteLine(i + " " + writer);
                    }
                }
                Assert.True(compressed.Length < tones.Length);
                var uncompressed = ToneCompressor.Decompress(compressed).ToArray();
                
                for (var i = 0; i < uncompressed.Length; i++)
                {
                    var tone = uncompressed[i];
                    using (var writer = new StringWriter())
                    {
                        tone.Log(writer);
                        _testOutputHelper.WriteLine(i + " " + writer);
                    }
                }
                Equal(tones, uncompressed);
            }
        }

        private void Equal(MediaToneMessage.Tone[] lefts, MediaToneMessage.Tone[] rights)
        {
            Assert.Equal(lefts.Length, rights.Length);
            for (int i = 0; i < lefts.Length; i++)
            {
                var left = lefts[i];
                var right = rights[i];
                Assert.Equal(left.CB1, right.CB1);
                Assert.Equal(left.CB2, right.CB2);
                Assert.Equal(left.CB3, right.CB3);
                Assert.Equal(left.CB4, right.CB4);
                Assert.Equal(left.Frequency1, right.Frequency1);
                Assert.Equal(left.Frequency2, right.Frequency2);
                Assert.Equal(left.Frequency3, right.Frequency3);
                Assert.Equal(left.Frequency4, right.Frequency4);
                Assert.Equal(left.Duration, right.Duration);
                Assert.Equal(left.CycleTo, right.CycleTo);
                Assert.Equal(left.CycleCount, right.CycleCount);
                Assert.Equal(left.Next, right.Next);
            }
        }
    }
}
