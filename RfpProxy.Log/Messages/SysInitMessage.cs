using System;
using System.IO;
using System.Net.NetworkInformation;
using RfpProxyLib;

namespace RfpProxy.Log.Messages
{
    public sealed class SysInitMessage : AaMiDeMessage
    {
        public ReadOnlyMemory<byte> Reserved1 { get; }

        public PhysicalAddress Mac { get; }

        public ReadOnlyMemory<byte> Reserved2 { get; }

        public string SwVersion { get; }

        public ReadOnlyMemory<byte> Reserved3 { get; }

        public SysInitMessage(ReadOnlyMemory<byte> data):base(MsgType.SYS_INIT, data)
        {
            Reserved1 = Raw.Slice(0x00, 0x08);
            Mac = new PhysicalAddress(Raw.Slice(0x08, 0x06).ToArray());
            Reserved2 = Raw.Slice(0x0e, 0x56);
            SwVersion = Raw.Slice(0x64, 0x90).Span.CString();
            Reserved3 = Raw.Slice(0xf4, 0x0f);
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($"Reserved1({HexEncoding.ByteToHex(Reserved1.Span)}) MAC({Mac}) ");
            writer.Write($"Reserved2({HexEncoding.ByteToHex(Reserved2.Span)}) SW Version({SwVersion}) ");
            writer.Write($"Reserved3({HexEncoding.ByteToHex(Reserved3.Span)})");
        }
    }
}