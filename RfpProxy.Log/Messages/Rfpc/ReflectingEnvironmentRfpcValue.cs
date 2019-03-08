using System;
using System.IO;

namespace RfpProxy.Log.Messages.Rfpc
{
    public sealed class ReflectingEnvironmentRfpcValue : DnmRfpcValue
    {
        public bool ReflectingEnvironment { get; }

        public override bool HasUnknown => false;

        public ReflectingEnvironmentRfpcValue(ReadOnlyMemory<byte> data):base(RfpcKey.ReflectingEnvironment)
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