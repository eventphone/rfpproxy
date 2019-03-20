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

        public override bool HasUnknown => true;

        protected override ReadOnlyMemory<byte> Raw => base.Raw.Slice(32);

        public SysAuthenticateMessage(ReadOnlyMemory<byte> data):base(MsgType.SYS_AUTHENTICATE, data)
        {
            Reserved1 = base.Raw.Slice(0, 8);
            RfpIv = base.Raw.Slice(8, 8);
            Reserved2 = base.Raw.Slice(16, 8);
            OmmIv = base.Raw.Slice(24, 8);
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($"Reserved1({Reserved1.ToHex()}) ");
            writer.Write($"RfpIv({RfpIv.ToHex()}) ");
            writer.Write($"Reserved2({Reserved2.ToHex()}) ");
            writer.Write($"OmmIv({OmmIv.ToHex()})");
        }
    }
}