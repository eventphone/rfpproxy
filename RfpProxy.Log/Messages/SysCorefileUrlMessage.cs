using System;
using System.IO;

namespace RfpProxy.Log.Messages
{
    public sealed class SysCorefileUrlMessage : AaMiDeMessage
    {
        public string Url { get; }

        public SysCorefileUrlMessage(ReadOnlyMemory<byte> data):base(MsgType.SYS_COREFILE_URL, data)
        {
            Url = Raw.Span.CString();
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($"Url({Url})");
        }
    }
}