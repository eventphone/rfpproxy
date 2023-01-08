using System;
using System.IO;
using RfpProxyLib;

namespace RfpProxy.AaMiDe.Sys
{
    public sealed class SysFirmwareUpdateMessage : AaMiDeMessage
    {
        public ReadOnlyMemory<byte> Reserved1 { get; }

        public string Value3 { get; }

        public string Value4 { get; }

        public string Value5 { get; }

        public string Value6 { get; }

        public string Value7 { get; }

        public byte Value8 { get; }

        public byte Value9 { get; }

        protected override ReadOnlyMemory<byte> Raw { get; }

        public SysFirmwareUpdateMessage(ReadOnlyMemory<byte> data):base(MsgType.SYS_FIRMWARE_UPDATE, data)
        {
            Raw = base.Raw;
            Reserved1 = Raw.Slice(0, 8);
            Raw = Raw.Slice(8);
            while (!Raw.IsEmpty)
            {
                var type = Raw.Span[0];
                var length = Raw.Span[1];
                var value = Raw.Slice(2, length);
                switch (type)
                {
                    case 3:
                        Value3 = value.Span.CString();
                        break;
                    case 4:
                        Value4 = value.Span.CString();
                        break;
                    case 5:
                        Value5 = value.Span.CString();
                        break;
                    case 6:
                        Value6 = value.Span.CString();
                        break;
                    case 7:
                        Value7 = value.Span.CString();
                        break;
                    case 8 when length == 1:
                        Value8 = value.Span[0];
                        break;
                    case 9 when length == 1:
                        Value9 = value.Span[0];
                        break;
                    default:
                        return;
                }
                Raw = Raw.Slice(2).Slice(length);
            }
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($"Value3({Value3}) " +
                         $"Value4({Value4}) " +
                         $"Value5({Value5}) " +
                         $"Value6({Value6}) " +
                         $"Value7({Value7}) " +
                         $"Value8({Value8}) " +
                         $"Value9({Value9}) ");
        }
    }
}