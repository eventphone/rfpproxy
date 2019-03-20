using System;
using System.IO;
using RfpProxyLib;

namespace RfpProxy.Log.Messages.Nwk.InformationElements
{
    public sealed class NwkIeModelIdentifier : NwkVariableLengthInformationElement
    {
        public ReadOnlyMemory<byte> Model { get; }

        public override bool HasUnknown => false;

        public NwkIeModelIdentifier(ReadOnlyMemory<byte> data) : base(NwkVariableLengthElementType.ModelIdentifier, data)
        {
            Model = data;
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($" Model({Model.ToHex()})");
        }
    }
}