using System;
using System.IO;
using RfpProxyLib;

namespace RfpProxy.Log.Messages
{
    public sealed class SysAuthenticateMessage : AaMiDeMessage
    {
        public ReadOnlyMemory<byte> Reserved1 { get; }

        public ReadOnlyMemory<byte> RfpIv { get; }
        
        public ReadOnlyMemory<byte> Reserved2 { get; }

        public ReadOnlyMemory<byte> OmmIv { get; }

        public SysAuthenticateMessage(ReadOnlyMemory<byte> data):base(MsgType.SYS_AUTHENTICATE, data)
        {
            Reserved1 = Raw.Slice(0, 8);
            RfpIv = Raw.Slice(8, 8);
            Reserved2 = Raw.Slice(16, 8);
            OmmIv = Raw.Slice(24, 8);
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($"Reserved1({HexEncoding.ByteToHex(Reserved1.Span)}) ");
            writer.Write($"RfpIv({HexEncoding.ByteToHex(RfpIv.Span)}) ");
            writer.Write($"Reserved2({HexEncoding.ByteToHex(Reserved2.Span)}) ");
            writer.Write($"OmmIv({HexEncoding.ByteToHex(OmmIv.Span)})");
        }
    }
}