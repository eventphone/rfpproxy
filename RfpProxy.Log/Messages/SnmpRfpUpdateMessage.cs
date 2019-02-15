using System;
using System.IO;
using System.Net;

namespace RfpProxy.Log.Messages
{
    public sealed class SnmpRfpUpdateMessage : AaMiDeMessage
    {
        public IPAddress Server { get; }

        public string Contact { get; }

        public string Location { get; }

        public string Name { get; }

        public string RoCommunity { get; }

        public string TrapCommunity { get; }

        public bool TrapEnabled { get; }

        public SnmpRfpUpdateMessage(ReadOnlyMemory<byte> data):base(MsgType.SNMP_RFP_UPDATE, data)
        {
            var span = base.Raw.Span;
            Server = new IPAddress(span.Slice(0, 4));
            Contact = span.Slice(0x4, 0x51).CString();
            Location = span.Slice(0x55, 0x51).CString();
            Name = span.Slice(0xa6, 0x51).CString();
            RoCommunity = span.Slice(0xf7, 0x29).CString();
            TrapCommunity = span.Slice(0x120, 0x29).CString();
            TrapEnabled = span[0x149] != 0;
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($"syscontact({Contact}) sysname({Name}) syslocation({Location}) rocommunity({RoCommunity}) trapcommunity({TrapCommunity}) trapenabled({TrapEnabled}) trapsink({Server})");
        }
    }
}