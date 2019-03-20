using System;
using System.Buffers.Binary;
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

        public ushort Capabilities { get; }

        public ReadOnlyMemory<byte> Reserved3 { get; }

        public string SwVersion { get; }

        public override bool HasUnknown => true;

        protected override ReadOnlyMemory<byte> Raw => base.Raw.Slice(0xf4);

        public SysInitMessage(ReadOnlyMemory<byte> data):base(MsgType.SYS_INIT, data)
        {
            Reserved1 = base.Raw.Slice(0x00, 0x08);
            Mac = new PhysicalAddress(base.Raw.Slice(0x08, 0x06).ToArray());
            Reserved2 = base.Raw.Slice(0x0e, 0x08);
            Capabilities = BinaryPrimitives.ReadUInt16BigEndian(base.Raw.Slice(0x16).Span);
            Reserved3 = base.Raw.Slice(0x18, 0x4c);

            SwVersion = base.Raw.Slice(0x64, 0x90).Span.CString();
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($"Reserved1({Reserved1.ToHex()}) MAC({Mac}) ");
            writer.Write($"Reserved2({Reserved2.ToHex()}) Capabilities({Capabilities:x2}) ");
            writer.Write($"Reserved3({Reserved3.ToHex()}) SW Version({SwVersion}) ");
        }
    }
}