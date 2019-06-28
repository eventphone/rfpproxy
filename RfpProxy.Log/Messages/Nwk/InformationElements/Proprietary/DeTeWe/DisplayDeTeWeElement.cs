using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RfpProxy.Log.Messages.Nwk.InformationElements.Proprietary.DeTeWe
{
    public class DisplayDeTeWeElement : DeTeWeElement
    {
        public enum DeTeWeDisplayType : byte
        {
            NoScroll = 0xb2,
            Scrollable = 0xb3,
            AddBottom = 0xb8,
            AddTop = 0xb7,
            Empty = 0xfe
        }

        public DeTeWeDisplayType DisplayType { get; }

        /// <summary>
        /// always seems to be 0xff
        /// </summary>
        public byte Padding { get; }

        /// <summary>
        /// seems to be always 2 entries
        /// </summary>
        public List<string> Values { get; }

        public override bool HasUnknown => !Enum.IsDefined(typeof(DeTeWeDisplayType), DisplayType) ||
                                           !Raw.IsEmpty ||
                                           Padding != 0xff;

        public override ReadOnlyMemory<byte> Raw { get; }

        public DisplayDeTeWeElement(ReadOnlyMemory<byte> data) : base(DeTeWeType.Display, data)
        {
            DisplayType = (DeTeWeDisplayType) data.Span[0];
            if (DisplayType == DeTeWeDisplayType.Empty && data.Length == 1)
            {
                Padding = 0xff;
                return;
            }
            Padding = data.Span[1];
            data = data.Slice(2);
            Values = new List<string>();
            while (data.Length > 0)
            {
                var length = data.Span[0];
                var value = data.Slice(1, length);
                Values.Add(Encoding.UTF8.GetString(value.Span));
                data = data.Slice(1).Slice(length);
            }
            Raw = data;
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            if (Enum.IsDefined(typeof(DeTeWeDisplayType), DisplayType))
                writer.Write($"({DisplayType:G})");
            else
                writer.Write($"({DisplayType:x})");
            if (HasUnknown)
                writer.Write($" Padding({Padding:x2})");
            foreach (var value in Values)
            {
                writer.WriteLine();
                writer.Write($"\t\t\t\t{value}");
            }
        }
    }
}