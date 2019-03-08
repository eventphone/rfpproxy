using System;
using System.IO;
using RfpProxyLib;

namespace RfpProxy.Log.Messages
{
    public sealed class SyncMessage : AaMiDeMessage
    {
        public ReadOnlyMemory<byte> Reserved1 { get; }

        public byte PayloadLength { get; }

        public ReadOnlyMemory<byte> Reserved2 { get; }

        public override bool HasUnknown => true;

        public SyncMessage(ReadOnlyMemory<byte> data) : base(MsgType.SYNC, data)
        {
            Reserved1 = Raw.Slice(0, 2);
            PayloadLength = Raw.Span[2];
            Reserved2 = Raw.Slice(3);
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($"Reserved1({Reserved1.ToHex()}) Length({PayloadLength}) Reserved2({Reserved2.ToHex()})");
        }
    }
}