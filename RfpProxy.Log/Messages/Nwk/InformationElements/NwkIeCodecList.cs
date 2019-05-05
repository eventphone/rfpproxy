using System;
using System.Collections.Generic;
using System.IO;

namespace RfpProxy.Log.Messages.Nwk.InformationElements
{
    public sealed class NwkIeCodecList : NwkVariableLengthInformationElement
    {
        public class Codec
        {
            public enum CodecIdentifier : byte
            {
                UserSpecific_32=0b0000_0001,
                G726Adpcm_32=0b0000_0010,
                G722_64=0b0000_0011,
                G711ALawPcm_64=0b0000_0100,
                G711ΜLawPcm_64=0b0000_0101,
                G7291_32=0b0000_0110,
                Mpeg4ErAacLd_32=0b0000_0111,
                Mpeg4ErAacLd_64=0b0000_1000,
                UserSpecific_64=0b0000_1001,
            }

            public enum MacDlcService : byte
            {
                Lu1Ina = 0b0000,
                Lu1Inb = 0b0001,
                Lu1Ipm = 0b0010,
                Lu1Ipq = 0b0011,
                Lu7Inb = 0b0100,
                Lu12Inb = 0b0101,
            }

            public enum CPlaneRouting : byte
            {
                CsOnly = 0b0000,
                CsPreferred = 0b0001,
                CfPreferred = 0b0010,
                CfOnly = 0b0100,
            }

            public enum SlotSize : byte
            {
                Half = 0b0000,
                Long640 = 0b0001,
                Long672 = 0b0010,
                Full = 0b0100,
                Double = 0b0101,
            }

            public CodecIdentifier Identifier { get; }

            public MacDlcService MacDlc { get; }

            public CPlaneRouting CPlane { get; }

            public SlotSize Size { get; }

            public Codec(ReadOnlySpan<byte> span)
            {
                Identifier = (CodecIdentifier) (span[0] & 0x7f);
                MacDlc = (MacDlcService) (span[1] & 0xf);
                CPlane = (CPlaneRouting) ((span[2] >> 4) & 0x7);
                Size = (SlotSize) (span[2] & 0xf);
            }
        }

        public bool Negotiation { get; }

        public IList<Codec> Codecs { get; }

        public override ReadOnlyMemory<byte> Raw { get; }

        public NwkIeCodecList(ReadOnlyMemory<byte> data) : base(NwkVariableLengthElementType.CodecList, data)
        {
            var span = data.Span;
            Negotiation = (span[0] & 0x10)!= 0;
            Codecs = new List<Codec>();
            span = span.Slice(1);
            data = data.Slice(1);
            do
            {
                Codecs.Add(new Codec(span));
                data = data.Slice(3);
                if (span[2] >= 128)
                    break;
                span = span.Slice(3);
            } while (true);
            Raw = data;
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($" Negotiation({Negotiation})");
            foreach (var codec in Codecs)
            {
                writer.Write($" {codec.Identifier}({codec.MacDlc}, {codec.CPlane}, {codec.Size})");
            }
        }
    }
}