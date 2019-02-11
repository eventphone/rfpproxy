﻿using System;
using System.Buffers.Binary;
using System.IO;
using System.Text;
using RfpProxyLib;

namespace RfpProxy.Log.Messages
{
    public abstract class AaMiDeMessage
    {
        public MsgType Type { get; }

        public ushort Length => BinaryPrimitives.ReadUInt16BigEndian(_data.Slice(2).Span);

        private readonly ReadOnlyMemory<byte> _data;

        protected virtual ReadOnlyMemory<byte> Raw => _data.Slice(4);

        protected AaMiDeMessage(MsgType type, ReadOnlyMemory<byte> data)
        {
            Type = type;
            _data = data;
        }

        public static AaMiDeMessage Create(ReadOnlyMemory<byte> data)
        {
            var type = (MsgType)BinaryPrimitives.ReadUInt16BigEndian(data.Slice(0, 2).Span);
            switch (type)
            {
                case MsgType.SYS_LED:
                    return new SysLedMessage(data);
                case MsgType.SYS_LICENSE_TIMER:
                    return new SysLicenseTimerMessage(data);
                case MsgType.MEDIA_AUDIO_STATISTICS:
                case MsgType.MEDIA_CHANNEL_MOD:
                case MsgType.MEDIA_CLOSE:
                case MsgType.MEDIA_DSP_CLOSE:
                case MsgType.MEDIA_EOS_DETECT:
                case MsgType.MEDIA_G729_USED:
                case MsgType.MEDIA_MUTE:
                case MsgType.MEDIA_REDIRECT_START:
                case MsgType.MEDIA_REDIRECT_STOP:
                case MsgType.MEDIA_RESTART:
                case MsgType.MEDIA_START:
                case MsgType.MEDIA_STATISTICS:
                case MsgType.MEDIA_STOP:
                case MsgType.MEDIA_TONE2:
                case MsgType.MEDIA_TRACE_PPN:
                case MsgType.MEDIA_VIDEO_STATE:
                    return new UnknownMediaMessage(type, data);
                case MsgType.MEDIA_OPEN:
                    return new OpenMediaMessage(data);
                case MsgType.MEDIA_DTMF:
                    return new DtmfMediaMessage(data);
                case MsgType.DNM:
                    return DnmMessage.Create(data);
                default:
                    return new UnknownAaMiDeMessage(type, data);
            }
        }

        public virtual void Log(TextWriter writer)
        {
            writer.Write($"{Type,-22}({Length,4}) ");
        }

        public static void PrintIfNotZero(TextWriter writer, string prefix, ReadOnlySpan<byte> data)
        {
            bool hasData = false;
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] != 0)
                {
                    hasData = true;
                    break;
                }
            }
            if (hasData)
            {
                writer.Write(prefix + HexEncoding.ByteToHex(data));
            }
        }
    }
}