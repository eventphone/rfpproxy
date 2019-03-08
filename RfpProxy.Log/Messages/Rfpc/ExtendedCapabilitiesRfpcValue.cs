using System;
using System.IO;

namespace RfpProxy.Log.Messages.Rfpc
{
    public sealed class ExtendedCapabilitiesRfpcValue : DnmRfpcValue
    {
        public bool FrequencyReplacementSupported { get; }

        public override bool HasUnknown { get; }

        public ExtendedCapabilitiesRfpcValue(ReadOnlyMemory<byte> data):base(RfpcKey.ExtendedCapabilities)
        {
            var span = data.Span;
            HasUnknown = span.IndexOf((byte) 16) != 1 || span.LastIndexOf((byte) 16) != 1;
            FrequencyReplacementSupported = (span[1] & 0x10) != 0;
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($" FrequencyReplacementSupported({FrequencyReplacementSupported})");
        }
    }
}