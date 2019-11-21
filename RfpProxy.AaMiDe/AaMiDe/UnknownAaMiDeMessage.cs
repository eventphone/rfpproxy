using System;
using System.IO;
using RfpProxyLib;

namespace RfpProxy.AaMiDe
{
    public sealed class UnknownAaMiDeMessage:AaMiDeMessage
    {
        public override bool HasUnknown => true;

        public UnknownAaMiDeMessage(MsgType type, ReadOnlyMemory<byte> data) : base(type, data)
        {
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write(Raw.ToHex());
        }
    }
}