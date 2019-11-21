using System;
using System.Buffers.Binary;
using System.IO;
using RfpProxyLib;

namespace RfpProxy.AaMiDe.Nwk.InformationElements
{
    public sealed class NwkIeModelIdentifier : NwkVariableLengthInformationElement
    {
        public ushort EMC { get; }

        public byte Model { get; }

        public ReadOnlyMemory<byte> IMEISV { get; }

        public override bool HasUnknown => false;

        public NwkIeModelIdentifier(ReadOnlyMemory<byte> data) : base(NwkVariableLengthElementType.ModelIdentifier, data)
        {
            if (data.Length == 3)
            {
                EMC = BinaryPrimitives.ReadUInt16BigEndian(data.Span);
                Model = data.Span[2];
            }
            else if (data.Length == 18)
            {
                IMEISV = data;
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            if (IMEISV.IsEmpty)
            {
                writer.Write($" EMC({EMC:x4}) Model({Model})");
            }
            else
            {
                writer.Write($" IMEISV({IMEISV.ToHex()})");
            }
        }
    }
}