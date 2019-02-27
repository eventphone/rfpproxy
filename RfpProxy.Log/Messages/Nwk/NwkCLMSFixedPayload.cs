using System;
using System.IO;
using RfpProxyLib;

namespace RfpProxy.Log.Messages.Nwk
{
    public sealed class NwkCLMSFixedPayload : NwkCLMSPayload
    {
        public ReadOnlyMemory<byte> Reserved { get; }

        public override bool HasUnknown => true;

        public NwkCLMSFixedPayload(byte ti, bool f, ReadOnlyMemory<byte> data):base(NwkProtocolDiscriminator.CLMS, ti, f)
        {
            Reserved = data;
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($" Reserved({Reserved.ToHex()})");
        }
    }
}