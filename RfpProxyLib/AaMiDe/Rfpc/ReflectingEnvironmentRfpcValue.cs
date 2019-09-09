using System;
using System.IO;

namespace RfpProxyLib.AaMiDe.Rfpc
{
    public sealed class ReflectingEnvironmentRfpcValue : DnmRfpcValue
    {
        public bool ReflectingEnvironment { get; }

        public override ReadOnlyMemory<byte> Raw => base.Raw.Slice(1);

        public ReflectingEnvironmentRfpcValue(ReadOnlyMemory<byte> data):base(RfpcKey.ReflectingEnvironment, data)
        {
            ReflectingEnvironment = data.Span[0] != 0;
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($" {ReflectingEnvironment}");
        }
    }
}