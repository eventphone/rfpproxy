using System;
using System.IO;

namespace RfpProxy.Log.Messages.Nwk
{
    public abstract class NwkPayload
    {
        public NwkProtocolDiscriminator ProtocolDiscriminator { get; }

        public byte TransactionIdentifier { get; }

        public bool TransactionFlag { get; }

        public abstract bool HasUnknown { get; }

        public NwkPayload(NwkProtocolDiscriminator pd, byte ti, bool f)
        {
            ProtocolDiscriminator = pd;
            TransactionIdentifier = ti;
            TransactionFlag = f;
        }

        public static NwkPayload Create(ReadOnlyMemory<byte> data)
        {
            var pd = (NwkProtocolDiscriminator)(data.Span[0] & 0xf);
            var ti = (byte) ((data.Span[0] >> 4) & 0b0111);
            var f = (data.Span[0] & 0b1000_0000) == 0;
            if (ti == 0b0111)
            {
                //TVX - ETSI EN 300 175-5 7.3
                ti = data.Span[1];
                data = data.Slice(1);
            }
            data = data.Slice(1);
            switch (pd)
            {
                case NwkProtocolDiscriminator.LCE:
                    throw new NotImplementedException("LCE");
                case NwkProtocolDiscriminator.CC:
                    return new NwkCCPayload(ti, f, data);
                case NwkProtocolDiscriminator.CISS:
                    return new NwkCISSPayload(ti, f, data);
                case NwkProtocolDiscriminator.MM:
                    return new NwkMMPayload(ti, f, data);
                case NwkProtocolDiscriminator.CLMS:
                    throw new NotImplementedException("CLMS");
                case NwkProtocolDiscriminator.COMS:
                    return new NwkCOMSPayload(ti, f, data);
                default:
                    if ((byte)pd > 8)
                        throw new NotImplementedException("Unknown");
                    throw new ArgumentOutOfRangeException();
            }
        }

        public virtual void Log(TextWriter writer)
        {
            writer.WriteLine();
            writer.Write($"\t{ProtocolDiscriminator,4}({(TransactionFlag?'o':'d')}{TransactionIdentifier})");
        }
    }
}