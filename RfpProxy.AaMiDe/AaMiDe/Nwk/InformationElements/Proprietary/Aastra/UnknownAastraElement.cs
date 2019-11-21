using System;
using System.IO;
using RfpProxyLib;

namespace RfpProxy.AaMiDe.Nwk.InformationElements.Proprietary.Aastra
{
    public class UnknownAastraElement : AastraElement
    {
        public override bool HasUnknown => true;

        public ReadOnlyMemory<byte> Raw { get; }

        public UnknownAastraElement(ReadOnlyMemory<byte> data)
        {
            Raw = data;
        }

        public override void Log(TextWriter writer)
        {
            writer.Write($"({Raw.ToHex()})");
        }
    }

    public class FirmwareAastraElement : AastraElement
    {
        public string Text1 { get; }
        
        public string Text2 { get; }

        public override bool HasUnknown => !Raw.IsEmpty;

        public ReadOnlyMemory<byte> Raw { get; }

        public FirmwareAastraElement(ReadOnlyMemory<byte> data)
        {
            var length = data.Span[0];
            data = data.Slice(1);
            Text1 = data.Slice(0, length).Span.CString();
            data = data.Slice(length);
            
            length = data.Span[0];
            data = data.Slice(1);
            Text2 = data.Slice(0, length).Span.CString();
            Raw = data.Slice(length);
        }

        public override void Log(TextWriter writer)
        {
            writer.Write($"Text1({Text1}) Text2({Text2})");
            if (HasUnknown)
                writer.Write($" Reserved({Raw.ToHex()})");
        }
    }

    public class Firmware2AastraElement : AastraElement
    {
        public string Text { get; }

        public override bool HasUnknown => !Raw.IsEmpty;

        public ReadOnlyMemory<byte> Raw { get; }

        public Firmware2AastraElement(ReadOnlyMemory<byte> data)
        {
            var length = data.Span[0];
            data = data.Slice(1);
            Text = data.Slice(0, length).Span.CString();
            Raw = data.Slice(length);
        }

        public override void Log(TextWriter writer)
        {
            writer.Write($"Text({Text})");
            if (HasUnknown)
                writer.Write($" Reserved({Raw.ToHex()})");
        }
    }

    public class UpdateAastraElement : AastraElement
    {
        public override bool HasUnknown => false;

        public ReadOnlyMemory<byte> Raw { get; }

        public UpdateAastraElement(ReadOnlyMemory<byte> data)
        {
            Raw = data;
        }

        public override void Log(TextWriter writer)
        {
            writer.Write($"Content({Raw.ToHex()})");
        }
    }

    public class UpdateAckAastraElement : AastraElement
    {
        public override bool HasUnknown => !Raw.IsEmpty;

        public ReadOnlyMemory<byte> Raw { get; }

        public UpdateAckAastraElement(ReadOnlyMemory<byte> data)
        {
            Raw = data;
        }

        public override void Log(TextWriter writer)
        {
            if (HasUnknown)
                writer.Write($"Content({Raw.ToHex()})");
        }
    }
}