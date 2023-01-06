using System;
using System.Buffers.Binary;
using System.IO;
using RfpProxy.AaMiDe.Media;
using RfpProxy.AaMiDe.Sys;
using RfpProxy.AaMiDe.Sync;
using RfpProxy.AaMiDe.Dnm;
using RfpProxyLib;

namespace RfpProxy.AaMiDe
{
    public abstract class AaMiDeMessage
    {
        private readonly ushort _length;

        public virtual ushort Length => 4;

        public virtual bool HasUnknown => !Raw.IsEmpty;

        public MsgType Type { get; }

        protected virtual ReadOnlyMemory<byte> Raw { get; }

        protected AaMiDeMessage(MsgType type)
        {
            Type = type;
            Raw = ReadOnlyMemory<byte>.Empty;
        }

        protected AaMiDeMessage(MsgType type, ReadOnlyMemory<byte> data):this(type)
        {
            var span = data.Span;
            _length = BinaryPrimitives.ReadUInt16BigEndian(span.Slice(2));
            Raw = data.Slice(4, _length);
        }

        public static AaMiDeMessage Create(ReadOnlyMemory<byte> data, RfpConnectionTracker reassembler)
        {
            var type = (MsgType)BinaryPrimitives.ReadUInt16BigEndian(data.Slice(0, 2).Span);
            switch (type)
            {
                case MsgType.SYS_LED:
                    return new SysLedMessage(data);
                case MsgType.SYS_LICENSE_TIMER:
                    return new SysLicenseTimerMessage(data);
                case MsgType.MEDIA_AUDIO_STATISTICS:
                case MsgType.MEDIA_BANDWIDTH_SWO:
                case MsgType.MEDIA_G729_USED:
                case MsgType.MEDIA_MUTE:
                case MsgType.MEDIA_TRACE_PPN:
                case MsgType.MEDIA_VIDEO_STATE:
                    return new UnknownMediaMessage(type, data);
                case MsgType.MEDIA_EOS_DETECT:
                    return new MediaEosDetectMessage(data);
                case MsgType.MEDIA_DSP_CLOSE:
                    return new MediaDspCloseMessage(data);
                case MsgType.MEDIA_REDIRECT_STOP:
                    return new MediaRedirectStopMessage(data);
                case MsgType.MEDIA_REDIRECT_START:
                    return new MediaRedirectStartMessage(data);
                case MsgType.MEDIA_RESTART:
                    return new MediaRestartMessage(data);
                case MsgType.MEDIA_TONE2:
                    return new MediaToneMessage(data);
                case MsgType.MEDIA_START:
                    return new MediaStartMessage(data);
                case MsgType.MEDIA_STOP:
                    return new MediaStopMessage(data);
                case MsgType.MEDIA_CLOSE:
                    return new MediaCloseMessage(data);
                case MsgType.MEDIA_CONF:
                    return new MediaConfMessage(data);
                case MsgType.MEDIA_OPEN:
                    return new MediaOpenMessage(data);
                case MsgType.MEDIA_DTMF:
                    return new MediaDtmfMessage(data);
                case MsgType.MEDIA_STATISTICS:
                    return new MediaStatisticsMessage(data);
                case MsgType.DNM:
                    return DnmMessage.CreateDnm(data, reassembler);
                case MsgType.SNMP_RFP_UPDATE:
                    return new SnmpRfpUpdateMessage(data);
                case MsgType.SYS_AUTHENTICATE:
                    return new SysAuthenticateMessage(data);
                case MsgType.SYS_INIT:
                    return new SysInitMessage(data);
                case MsgType.ACK:
                    return new AckMessage(data);
                case MsgType.NACK:
                    return new NackMessage(data);
                case MsgType.SYS_HEARTBEAT_INTERVAL:
                    return new SysHeartbeatIntervalMessage(data);
                case MsgType.SYS_IP_OPTIONS:
                    return new SysIpOptionsMessage(data);
                case MsgType.SYS_HTTP_SET:
                    return new SysHttpSetMessage(data);
                case MsgType.SYS_SYSLOG:
                    return new SysSyslogMessage(data);
                case MsgType.SYS_CORE_DUMP:
                    return new SysCorefileUrlMessage(data);
                case MsgType.SYS_PASSWD:
                    return new SysPasswdMessage(data);
                case MsgType.SYS_RPING:
                    return new SysRPingMessage(data);
                case MsgType.SYS_VSNTP_TIME:
                    return new SysRoundtripDelayMessage(data);
                case MsgType.SYS_RANDOM_VALUE:
                    return new SysRandomValueMessage(data);
                case MsgType.SYS_RESET:
                    return new SysResetMessage(data);
                case MsgType.HEARTBEAT:
                    return new HeartbeatMessage(data);
                case MsgType.SYNC:
                    return SyncMessage.Create(data);
                case MsgType.SYS_NEW_SW:
                    return new SysNewSwMessage(data);
                case MsgType.SYS_OMM_CONTROL:
                    return new SysOmmControlMessage(data);
                case MsgType.SYS_MAX_CHANNELS:
                    return new SysMaxChannelsMessage(data);
                case MsgType.SYS_ENCRYPTION_CONF:
                    return new SysEncryptionConf(data);
                default:
                    return new UnknownAaMiDeMessage(type, data);
            }
        }

        public virtual void Log(TextWriter writer)
        {
            writer.Write($"{Type,-22}({_length,4}) ");
            if (!Raw.IsEmpty)
            {
                writer.Write($"Reserved({Raw.ToHex()}) ");
            }
        }

        public virtual Span<byte> Serialize(Span<byte> data)
        {
            BinaryPrimitives.WriteUInt16BigEndian(data, (ushort) Type);
            BinaryPrimitives.WriteUInt16BigEndian(data.Slice(2), (ushort) (Length - 4));
            return data.Slice(4);
        }
    }
}