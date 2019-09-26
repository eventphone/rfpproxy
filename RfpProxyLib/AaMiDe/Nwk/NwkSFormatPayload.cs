using System;
using System.Collections.Generic;
using System.IO;
using RfpProxyLib.AaMiDe.Nwk.InformationElements;

namespace RfpProxyLib.AaMiDe.Nwk
{
    public abstract class NwkSFormatPayload : NwkPayload
    {
        public IList<NwkInformationElement> InformationElements { get; }

        public override bool HasUnknown { get; }

        protected NwkSFormatPayload(NwkProtocolDiscriminator pd, byte ti, bool f, ReadOnlyMemory<byte> data) : base(pd, ti, f)
        {
            var span = data.Span;
            InformationElements = new List<NwkInformationElement>();
            int i = 0;
            for (; i < span.Length; i++)
            {
                var current = span[i];
                if ((current & 0x80) != 0)
                {
                    //fixed length
                    if ((current & 0xf0) == 0xe0)
                    {
                        //double byte
                        var identifier = (byte) (current & 0xf);
                        var content = span[i + 1];
                        var ie = NwkDoubleByteInformationElement.Create(identifier, content);
                        if (ie.HasUnknown)
                            HasUnknown = true;
                        InformationElements.Add(ie);
                        i++;
                    }
                    else
                    {
                        //single byte
                        var identifier = (byte) ((current >> 4) & 0x7);
                        var content = (byte)(current & 0xf);
                        var ie = NwkSingleByteInformationElement.Create(identifier, content);
                        if (ie.HasUnknown)
                            HasUnknown = true;
                        InformationElements.Add(ie);
                    }
                }
                else
                {
                    var type = (NwkVariableLengthElementType) current;
                    var length = span[i+1];
                    if (length > 0) //EN ETSI 300 175-5 #7.5.1
                    {
                        var ie = NwkVariableLengthInformationElement.Create(type, data.Slice(i + 2, length));
                        if (ie.HasUnknown)
                            HasUnknown = true;
                        InformationElements.Add(ie);
                    }
                    else
                    {
                        InformationElements.Add(new NwkIeEmpty());
                    }
                    i += length + 1;
                }
            }
            if (i < span.Length)
                HasUnknown = true;
        }

        protected abstract string MessageType { get; }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($" {MessageType}");
            foreach (var ie in InformationElements)
            {
                writer.WriteLine();
                ie.Log(writer);
            }
        }
    }
}