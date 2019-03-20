using System;
using System.IO;
using RfpProxyLib;

namespace RfpProxy.Log.Messages.Nwk.InformationElements
{
    public sealed class NwkIeLocationArea : NwkVariableLengthInformationElement
    {
        public bool HasLocationAreaLevel { get; }

        public bool HasExtendedLocationInformation { get; }

        public byte LocationAreaLevel { get; }

        public override bool HasUnknown => HasExtendedLocationInformation;

        public ReadOnlyMemory<byte> Reserved { get; }

        public NwkIeLocationArea(ReadOnlyMemory<byte> data) : base(NwkVariableLengthElementType.LocationArea, data)
        {
            var span = data.Span;
            HasExtendedLocationInformation = (span[0] & 0x80) != 0;
            HasLocationAreaLevel = (span[0] & 0x40) != 0;
            if (HasLocationAreaLevel)
                LocationAreaLevel = (byte) (span[0] & 0x3f);
            if (HasExtendedLocationInformation)
            {
                Reserved = data.Slice(1);
            }
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            if (HasLocationAreaLevel)
                writer.Write($" LocationAreaLevel({LocationAreaLevel})");
            if (HasExtendedLocationInformation)
                writer.Write($" Reserved({Reserved.ToHex()})");
        }
    }
}