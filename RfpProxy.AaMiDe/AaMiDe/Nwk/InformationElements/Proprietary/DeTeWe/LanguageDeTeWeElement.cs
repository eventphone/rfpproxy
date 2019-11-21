using System;
using System.IO;

namespace RfpProxy.AaMiDe.Nwk.InformationElements.Proprietary.DeTeWe
{
    public class LanguageDeTeWeElement : DeTeWeElement
    {
        public enum LanguageCode : byte
        {
            De = 0x01,
            En = 0x02,
            Fr = 0x03,
            Es = 0x04,
            It = 0x05,
            Nl = 0x06,
            Sv = 0x07,
            Dk = 0x08,
            Pt = 0x09,
            No = 0x0a,
            Fi = 0x0b,
            Hu = 0x0c,
            Cs = 0x0d,
            Sl = 0x0e,
            Ru = 0x0f,
            Tr = 0x10,
            Pl = 0x11,
        }

        public LanguageCode Language { get; }

        public override ReadOnlyMemory<byte> Raw => base.Raw.Slice(1);

        public LanguageDeTeWeElement(ReadOnlyMemory<byte> data):base(DeTeWeType.Language, data)
        {
            if (data.Length != 1)
            {
                throw new ArgumentOutOfRangeException();
            }

            Language = (LanguageCode) data.Span[0];
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($"({Language:G})");
        }
    }
}