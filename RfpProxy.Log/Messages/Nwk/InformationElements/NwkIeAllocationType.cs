using System;
using System.IO;

namespace RfpProxy.Log.Messages.Nwk.InformationElements
{
    public sealed class NwkIeAllocationType : NwkVariableLengthInformationElement
    {
        public enum AlgorithmIdentifierCode : byte
        {
            Dsaa = 0b0000_0001,
            Dsaa2 = 0b0000_0010
        }

        public enum Relation : byte
        {
            IPUI = 0b0,
            PARK = 0b1,
        }

        public AlgorithmIdentifierCode AlgorithmIdentifier { get; }

        public byte UakNumber { get; }

        public Relation UakRelation { get; }

        public byte AcNumber { get; }

        public Relation AcRelation { get; }

        public override ReadOnlyMemory<byte> Raw => base.Raw.Slice(2);

        public NwkIeAllocationType(ReadOnlyMemory<byte> data):base(NwkVariableLengthElementType.AllocationType, data)
        {
            var span = data.Span;
            AlgorithmIdentifier = (AlgorithmIdentifierCode) span[0];
            UakNumber = (byte) ((span[1] >> 4) & 0x07);
            UakRelation = (Relation) (span[1] >> 7);
            AcNumber = (byte) (span[1] & 0x07);
            AcRelation = (Relation) ((span[1] & 0x08) >> 3);
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($" Algorithm({AlgorithmIdentifier:G}) UAKi({UakNumber}, {UakRelation}) ACi({AcNumber}, {AcRelation})");
        }
    }
}