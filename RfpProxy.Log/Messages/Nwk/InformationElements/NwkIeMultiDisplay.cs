using System;
using System.IO;

namespace RfpProxy.Log.Messages.Nwk.InformationElements
{
    public sealed class NwkIeMultiDisplay : NwkVariableLengthInformationElement
    {

        public override bool HasUnknown => false;

        public ReadOnlyMemory<byte> Keys { get; }

        public NwkIeMultiDisplay(ReadOnlyMemory<byte> data) : base(NwkVariableLengthElementType.MultiDisplay, data)
        {
            Keys = data;
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            if (Keys.IsEmpty) return;
            writer.Write(" Keys(");
            foreach (var key in Keys.Span)
            {
                if (key < 0x1f)
                {
                    writer.Write('{');
                    writer.Write(((DECTControlCodes)key).ToString("G"));
                    writer.Write('}');
                }
                else if (key > 128)
                {
                    writer.Write('{');
                    writer.Write(key.ToString("x2"));
                    writer.Write('}');
                }
                else
                {
                    writer.Write((char)key);
                }
            }
            writer.Write(")");
        }
    }
}