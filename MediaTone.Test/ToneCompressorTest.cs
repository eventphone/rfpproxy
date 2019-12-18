using System;
using System.IO;
using System.Linq;
using RfpProxy.MediaTone;
using RfpProxy.AaMiDe.Media;
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
        [InlineData("elise")]
        [InlineData("tetris")]
        public void CanCompress(string name)
        {
            var c = new MidiReader(name);
            var tones = c.GetTones().ToArray();
            Compress(tones, false, true, false);
        }

        [Theory]
        [InlineData("amelie")]
        [InlineData("portal")]
        [InlineData("smb")]
        [InlineData("elise")]
        [InlineData("tetris")]
        public void CanCompressWithLimit(string name)
        {
            var c = new MidiReader(name);
            var tones = c.GetTones().ToArray();
            Compress(tones, false, true, false, 256);
        }

        [Fact]
        public void CanCompress3Cycles()
        {
            var tones = new []
            {
                new RelativeTone(1),
                new RelativeTone(2),
                new RelativeTone(3),
                new RelativeTone(1),
                new RelativeTone(2),
                new RelativeTone(3),
                new RelativeTone(1),
                new RelativeTone(2),
                new RelativeTone(3),
            };
            var count = Compress(tones.Select((x,i)=>x.Tone(i)).ToArray(), false, true, false);
            Assert.Equal(3, count);
        }

        [Fact]
        public void CanMergeTones()
        {
            var tones = new []
            {
                new RelativeTone(1),
                new RelativeTone(1),
                new RelativeTone(1),
                new RelativeTone(3),
                new RelativeTone(4),
                new RelativeTone(4),
                new RelativeTone(4),
                new RelativeTone(2),
            };
            var count = Compress(tones.Select((x,i)=>x.Tone(i)).ToArray(), false, true, false);
            Assert.Equal(4, count);
        }

        [Fact]
        public void CanCompressInsideCycle()
        {
            var tones = new []
            {
                new RelativeTone(0),
                new RelativeTone(7),
                new RelativeTone(1),
                new RelativeTone(2),
                new RelativeTone(3),
                new RelativeTone(1),
                new RelativeTone(2),
                new RelativeTone(3),
                new RelativeTone(8),
                new RelativeTone(4),
                new RelativeTone(7),
                new RelativeTone(1),
                new RelativeTone(2),
                new RelativeTone(3),
                new RelativeTone(1),
                new RelativeTone(2),
                new RelativeTone(3),
                new RelativeTone(8),
                new RelativeTone(5),
            };
            var count = Compress(tones.Select((x,i)=>x.Tone(i)).ToArray(), false, true, false);
            Assert.Equal(9, count);
        }

        [Fact]
        public void CanCompressInsideCycleWithGap()
        {
            var tones = new []
            {
                new RelativeTone(0),
                new RelativeTone(7),
                new RelativeTone(1),
                new RelativeTone(2),
                new RelativeTone(3),
                new RelativeTone(14),
                new RelativeTone(1),
                new RelativeTone(2),
                new RelativeTone(3),
                new RelativeTone(8),
                new RelativeTone(4),
                new RelativeTone(7),
                new RelativeTone(1),
                new RelativeTone(2),
                new RelativeTone(3),
                new RelativeTone(14),
                new RelativeTone(1),
                new RelativeTone(2),
                new RelativeTone(3),
                new RelativeTone(8),
                new RelativeTone(5),
            };
            var count = Compress(tones.Select((x,i)=>x.Tone(i)).ToArray(), false, true, false);
            Assert.Equal(10, count);
        }

        private int Compress(MediaToneMessage.Tone[] tones, bool showBefore, bool showcompressed, bool showAfter, int limit = Int32.MaxValue)
        {
            var compressor = new ToneCompressor(tones, limit);
            var compressed = compressor.Compress();
            Assert.True(compressed.Length < tones.Length);
            Assert.True(compressed.Length <= limit);
            var uncompressed = ToneCompressor.Decompress(compressed).ToArray();
            _testOutputHelper.WriteLine($"{tones.Length} > {compressed.Length} < {uncompressed.Length}");
            if (limit < Int32.MaxValue && tones.Length > uncompressed.Length)
                tones = ToneCompressor.Merge(tones).AsMemory(0, uncompressed.Length).ToArray();
            if (showBefore)
            {
                for (var i = 0; i < tones.Length; i++)
                {
                    var tone = tones[i];
                    using (var writer = new StringWriter())
                    {
                        tone.Log(writer);
                        _testOutputHelper.WriteLine(i + " " + writer);
                    }
                }
                _testOutputHelper.WriteLine(String.Empty);
            }
            if (showcompressed)
            {
                for (var i = 0; i < compressed.Length; i++)
                {
                    var tone = compressed[i];
                    using (var writer = new StringWriter())
                    {
                        tone.Log(writer);
                        _testOutputHelper.WriteLine(i + " " + writer);
                    }
                }
                _testOutputHelper.WriteLine(String.Empty);
            }
            if (showAfter)
            {
                for (var i = 0; i < uncompressed.Length; i++)
                {
                    var tone = uncompressed[i];
                    using (var writer = new StringWriter())
                    {
                        tone.Log(writer);
                        _testOutputHelper.WriteLine(i + " " + writer);
                    }
                }
                _testOutputHelper.WriteLine(String.Empty);
            }
            tones = ToneCompressor.Merge(tones);
            Equal(tones, uncompressed);
            return compressed.Length;
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
