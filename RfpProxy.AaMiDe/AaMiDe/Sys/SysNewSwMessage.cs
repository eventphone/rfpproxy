using System;
using System.IO;
using RfpProxyLib;

namespace RfpProxy.AaMiDe.Sys
{
    public sealed class SysNewSwMessage : AaMiDeMessage
    {
        public override bool HasUnknown => Raw.Length != 1 || Raw.Span[0] != 0;

        public SysNewSwMessage(ReadOnlyMemory<byte> data):base(MsgType.SYS_NEW_SW, data)
        {
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write(Raw.ToHex());
        }
    }
}