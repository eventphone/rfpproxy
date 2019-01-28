using System;
using System.IO;

namespace RfpProxy.Log.Messages.Dnm
{
    public sealed class MacEncEksIndPayload : DnmPayload
    {
        public enum MacEncEksIndFlag : byte
        {
            Encrypted = 1,
        }

        public MacEncEksIndFlag Flag { get; }

        public MacEncEksIndPayload(ReadOnlyMemory<byte> data):base(data)
        {
            Flag = (MacEncEksIndFlag) data.Span[0];
        }

        public override void Log(TextWriter writer)
        {
            writer.Write($"\tMAC: Flag({Flag:G})");
        }
    }
}