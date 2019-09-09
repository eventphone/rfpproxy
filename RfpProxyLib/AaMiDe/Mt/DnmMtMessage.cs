using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RfpProxyLib.AaMiDe.Dnm;

namespace RfpProxyLib.AaMiDe.Mt
{
    public enum DnmMtType : byte
    {
    }

    public enum MtKey : byte
    {
    }

    public sealed class DnmMtMessage : AaMiDeMessage
    {
        public DnmLayer Layer { get; }

        public DnmMtType DnmType { get; }

        public List<DnmMtValue> Values { get; }

        public override bool HasUnknown => Values.Any(x=>x.HasUnknown);

        protected override ReadOnlyMemory<byte> Raw { get; }

        public DnmMtMessage(ReadOnlyMemory<byte> data) : base(MsgType.DNM, data)
        {
            var span = base.Raw.Span;
            Layer = (DnmLayer) span[0];
            DnmType = (DnmMtType) span[1];
            Values = new List<DnmMtValue>();

            var payload = base.Raw.Slice(2);
            while (payload.Length > 0)
            {
                var key = (MtKey) payload.Span[0];
                var length = payload.Span[1];
                var value = payload.Slice(2, length);
                Values.Add(DnmMtValue.Create(key, value));
                payload = payload.Slice(2).Slice(length);
            }
            Raw = payload;
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($"Layer({Layer,-3:G}) Type({DnmType,-20:G})");
            foreach (var value in Values)
            {
                value.Log(writer);
            }
        }
    }
}