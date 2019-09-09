using System;
using System.Buffers.Binary;
using System.IO;

namespace RfpProxyLib.AaMiDe.Nwk.InformationElements
{
    public sealed class NwkIeAuthType : NwkVariableLengthInformationElement
    {
        public enum AuthenticationAlgorithm : byte
        {
            DSAA = 0b0000_0001,
            DSAA2 = 0b0000_0010,
            Gsm = 0b0100_0000,
            Umts = 0b0010_0000,
            Proprietary = 0b0111_1111,
        }

        public enum AuthenticationKeyType : byte
        {
            UserAuthenticationKey = 0b001,
            UserPersonalIdentity = 0b0011,
            AuthenticationCode = 0b0100
        }

        public AuthenticationAlgorithm Algorithm { get; }

        public byte Proprietary { get; }

        public AuthenticationKeyType KeyType { get; }

        public byte KeyNumber { get; }

        /// <summary>
        /// Increment value of the ZAP field
        /// </summary>
        public bool Inc { get; }

        /// <summary>
        /// generated derived cipher key shall be used as default cipher key for early encryption
        /// </summary>
        public bool Def { get; }

        /// <summary>
        /// Include the derived cipher key in the AUTHENTICATION-REPLY message
        /// </summary>
        public bool Txc { get; }

        /// <summary>
        /// store the derived cipher key
        /// </summary>
        public bool Upc { get; }

        /// <summary>
        /// If the UPC bit is set to 1, then this field contains the binary coded number which is given to the newly derived cipher key
        /// If the UPC bit is set to 0, then this field is not applicable and should be set to 0
        /// </summary>
        public byte CipherKeyNumber { get; }

        public ushort DefaultCipherKey { get; }

        public override ReadOnlyMemory<byte> Raw => Def ? base.Raw.Slice(5) : base.Raw.Slice(3);

        public NwkIeAuthType(ReadOnlyMemory<byte> data) : base(NwkVariableLengthElementType.AuthType, data)
        {
            var span = data.Span;
            Algorithm = (AuthenticationAlgorithm) span[0];
            span = span.Slice(1);
            if (Algorithm == AuthenticationAlgorithm.Proprietary)
            {
                Proprietary = span[0];
                span = span.Slice(1);
            }
            KeyType = (AuthenticationKeyType) (span[0] >> 4);
            KeyNumber = (byte) (span[0] & 0xf);
            Inc = (span[1] & 0x80) != 0;
            Def = (span[1] & 0x40) != 0;
            Txc = (span[1] & 0x20) != 0;
            Upc = (span[1] & 0x10) != 0;
            CipherKeyNumber = (byte) (span[1] & 0xf);
            if (Def)
            {
                DefaultCipherKey = BinaryPrimitives.ReadUInt16BigEndian(span.Slice(2));
            }
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($" {Algorithm}(");
            if (Algorithm == AuthenticationAlgorithm.Proprietary)
            {
                writer.Write($"{Proprietary}, ");
            }
            writer.Write($"{KeyType}, {KeyNumber}) INC({Inc}) DEF({Def}) TXC({Txc}) UPC({Upc})");
            if (Upc)
                writer.Write($" CipherKeyNumber({CipherKeyNumber})");
            if (Def)
                writer.Write($" DefaultCipherKeyIndex({DefaultCipherKey})");
        }
    }
}