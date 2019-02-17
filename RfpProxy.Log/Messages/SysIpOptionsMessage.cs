using System;
using System.IO;
using RfpProxyLib;

namespace RfpProxy.Log.Messages
{
    public sealed class SysIpOptionsMessage : AaMiDeMessage
    {
        public byte VoiceTos { get; }
        
        public byte SignalTos { get; }

        public byte Ttl { get; }

        public byte SignalVlanPriority { get; }
        
        public byte VoiceVlanPriority { get; }

        public ReadOnlyMemory<byte> Reserved { get; }

        public SysIpOptionsMessage(ReadOnlyMemory<byte> data):base(MsgType.SYS_IP_OPTIONS, data)
        {
            var span = Raw.Span;
            VoiceTos = span[0];
            SignalTos = span[1];
            Ttl = span[2];
            SignalVlanPriority = span[3];
            VoiceVlanPriority = span[4];
            Reserved = Raw.Slice(5);
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($"VoiceTOS(0x{VoiceTos:x2}) SignalTOS(0x{SignalTos:x2}) TTL({Ttl}) ");
            writer.Write($"SignalVlanPrio({SignalVlanPriority}) VoiceVlanPrio({VoiceVlanPriority}) ");
            writer.Write($"Reserved({HexEncoding.ByteToHex(Reserved.Span)})");
        }
    }
}