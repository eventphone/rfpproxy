using System;
using System.IO;

namespace RfpProxy.Log.Messages.Nwk.InformationElements
{
    public sealed class NwkIeCipherInfo : NwkVariableLengthInformationElement
    {
        public enum CipherKeyType : byte
        {
            Derived = 0b1001,
            Static = 0b1010
        }

        public enum CipherAlgorithm : byte
        {
            Dsc = 0b0000_0001,
            Dsc2 = 0b0000_0010,
            GprsNotUsed = 0b0010_1000,
            Gea1 = 0b0010_1001,
            Gea2 = 0b0010_1010,
            Gea3 = 0b0010_1011,
            Gea4 = 0b0010_1100,
            Gea5 = 0b0010_1101,
            Gea6 = 0b0010_1110,
            Gea7 = 0b0010_1111,
            Proprietary = 0b0111_1111,
        }

        public bool Enabled { get; }

        public CipherAlgorithm Algorithm { get; }

        public byte Proprietary { get; }

        public CipherKeyType KeyType { get; }

        public byte KeyNumber { get; }

        public override ReadOnlyMemory<byte> Raw => Algorithm == CipherAlgorithm.Proprietary ? base.Raw.Slice(3) : base.Raw.Slice(2);

        public NwkIeCipherInfo(ReadOnlyMemory<byte> data) : base(NwkVariableLengthElementType.CipherInfo, data)
        {
            var span = data.Span;
            Enabled = (span[0] & 0x80) == 0;
            Algorithm = (CipherAlgorithm) (span[0] & 0x7f);
            span = span.Slice(1);
            if (Algorithm == CipherAlgorithm.Proprietary)
            {
                Proprietary = span[0];
                span = span.Slice(1);
            }
            KeyType = (CipherKeyType) (span[0] >> 4);
            KeyNumber = (byte) (span[0] & 0xf);
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($" {Algorithm}(");
            if (Algorithm == CipherAlgorithm.Proprietary)
            {
                writer.Write($"{Proprietary}, ");
            }
            writer.Write($"{KeyType}, {KeyNumber})");
        }
    }
}