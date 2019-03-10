using System;
using System.Buffers.Binary;
using System.IO;
using RfpProxyLib;

namespace RfpProxy.Log.Messages.Nwk.InformationElements
{
    public sealed class NwkIeEscape2Proprietary : NwkVariableLengthInformationElement
    {
        public enum DiscriminatorType : byte
        {
            Unspecified = 0b1000_0000,
            EMC = 0b1000_0001
        }

        public DiscriminatorType Discriminator { get; }
        
        /// <summary>
        /// Equipment Manufacturer Code
        /// </summary>
        public ushort EMC { get; }

        public ReadOnlyMemory<byte> Proprietary { get; }

        public override bool HasUnknown => true;

        public NwkIeEscape2Proprietary(ReadOnlyMemory<byte> data) : base(NwkVariableLengthElementType.Escape2Proprietary)
        {
            Discriminator = (DiscriminatorType) data.Span[0];
            if (Discriminator == DiscriminatorType.EMC)
            {
                EMC = BinaryPrimitives.ReadUInt16BigEndian(data.Span.Slice(1));
                Proprietary = data.Slice(3);
            }
            else
            {
                Proprietary = data.Slice(1);
            }
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            if (Discriminator == DiscriminatorType.EMC)
                writer.Write($" EMC({EMC:x4}) Proprietary({Proprietary.ToHex()})");
            else
                writer.Write($" {Discriminator}({Proprietary.ToHex()})");
        }
    }
}