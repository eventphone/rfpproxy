using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RfpProxy.Log.Messages.Nwk.InformationElements.Proprietary.DeTeWe
{
    public class DisplayDeTeWeElement : DeTeWeElement
    {
        /// <summary>
        /// byte 2 always seems to be 0xff
        /// byte 1:
        /// 0xb2 - can't scroll
        /// 0xb3 - scrollable
        /// 0xb8 - add bottom
        /// 0xb7 - add top
        /// </summary>
        public ushort Reserved { get; }

        /// <summary>
        /// seems to be always 2 entries
        /// </summary>
        public List<string> Values { get; }

        public override bool HasUnknown => true;

        public DisplayDeTeWeElement(ReadOnlyMemory<byte> data) : base(DeTeWeType.Display, data)
        {
            Reserved = BinaryPrimitives.ReadUInt16BigEndian(data.Span);
            data = data.Slice(2);
            Values = new List<string>();
            while (data.Length > 0)
            {
                var length = data.Span[0];
                var value = data.Slice(1, length);
                Values.Add(Encoding.UTF8.GetString(value.Span));
                data = data.Slice(1).Slice(length);
            }
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($"({String.Join('|', Values)}) Reserved({Reserved:x4})");
        }
    }
}