using System;
using System.IO;

namespace RfpProxy.AaMiDe.Sys
{
    public sealed class SysIpOptionsMessage : AaMiDeMessage
    {
        public byte VoiceTos { get; }
        
        public byte SignalTos { get; }

        public byte Ttl { get; }

        public byte SignalVlanPriority { get; }
        
        public byte VoiceVlanPriority { get; }

        /// <summary>
        /// padding
        /// </summary>
        protected override ReadOnlyMemory<byte> Raw => base.Raw.Slice(5);

        public override bool HasUnknown => false;

        public SysIpOptionsMessage(ReadOnlyMemory<byte> data):base(MsgType.SYS_IP_OPTIONS, data)
        {
            var span = base.Raw.Span;
            VoiceTos = span[0];
            SignalTos = span[1];
            Ttl = span[2];
            SignalVlanPriority = span[3];
            VoiceVlanPriority = span[4];
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($"VoiceTOS(0x{VoiceTos:x2}) SignalTOS(0x{SignalTos:x2}) TTL({Ttl}) ");
            writer.Write($"SignalVlanPrio({SignalVlanPriority}) VoiceVlanPrio({VoiceVlanPriority})");
        }
    }
}