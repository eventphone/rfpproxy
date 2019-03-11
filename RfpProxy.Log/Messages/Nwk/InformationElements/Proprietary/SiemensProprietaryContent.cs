using System;
using System.Collections.Generic;
using System.IO;
using RfpProxyLib;

namespace RfpProxy.Log.Messages.Nwk.InformationElements.Proprietary
{
    /// <summary>
    /// http://dect.osmocom.org/wiki/Siemens
    /// </summary>
    public class SiemensProprietaryContent : NwkIeProprietaryContent
    {
        public enum SiemensType : byte
        {
            CallerId = 0x28,
            TimeDate = 0x3b,
            Display = 0x54,
            Unnamed = 0x58,
            CissAcknowledgementSeq = 0x59,
            CissRequestSeq= 0x5b ,
        }

        public class SiemensElement
        {
            public SiemensType Type { get; }

            public ReadOnlyMemory<byte> Raw { get; }

            public SiemensElement(SiemensType type, ReadOnlyMemory<byte> data)
            {
                Type = type;
                Raw = data;
            }
        }

        public List<SiemensElement> Elements { get; }

        public override bool HasUnknown => true;

        public SiemensProprietaryContent(ReadOnlyMemory<byte> data)
        {
            Elements = new List<SiemensElement>();
            while (data.Length > 0)
            {
                var length = data.Span[1];
                Elements.Add(new SiemensElement((SiemensType) data.Span[0], data.Slice(2,length)));
                data = data.Slice(2).Slice(length);
            }
        }

        public override void Log(TextWriter writer)
        {
            foreach (var element in Elements)
            {
                writer.WriteLine();
                writer.Write("\t\t\t");
                if (Enum.IsDefined(typeof(SiemensType), element.Type))
                    writer.Write(element.Type.ToString("G"));
                else
                    writer.Write(element.Type.ToString("x"));
                writer.Write($"({element.Raw.ToHex()})");
            }
        }
    }
}