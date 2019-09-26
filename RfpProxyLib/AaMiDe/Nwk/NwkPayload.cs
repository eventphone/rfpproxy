using System;
using System.IO;

namespace RfpProxyLib.AaMiDe.Nwk
{
    public abstract class NwkPayload
    {
        public NwkProtocolDiscriminator ProtocolDiscriminator { get; }

        public byte TransactionIdentifier { get; }

        public bool TransactionFlag { get; }

        public abstract bool HasUnknown { get; }

        public bool WasRetransmitted { get; set; }

        protected NwkPayload(NwkProtocolDiscriminator pd, byte ti, bool f)
        {
            ProtocolDiscriminator = pd;
            TransactionIdentifier = ti;
            TransactionFlag = f;
        }

        public static NwkPayload Create(ReadOnlyMemory<byte> data)
        {
            if (data.IsEmpty) return new NwkEmptyPayload();
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
                    return NwkLCEPayload.Create(ti, f, data);
                case NwkProtocolDiscriminator.CC:
                    return new NwkCCPayload(ti, f, data);
                case NwkProtocolDiscriminator.CISS:
                    return new NwkCISSPayload(ti, f, data);
                case NwkProtocolDiscriminator.MM:
                    return new NwkMMPayload(ti, f, data);
                case NwkProtocolDiscriminator.CLMS:
                    return NwkCLMSPayload.Create(ti, f, data);
                case NwkProtocolDiscriminator.COMS:
                    return new NwkCOMSPayload(ti, f, data);
                default:
                    if ((byte)pd > 8)
                        throw new InvalidProtocolDiscriminatorException();
                    throw new ArgumentOutOfRangeException("Protocol Discriminator (ETSI EN 300 175-5 section 7.2)");
            }
        }

        public virtual void Log(TextWriter writer)
        {
            writer.WriteLine();
            writer.Write($"\t{ProtocolDiscriminator,4}({(TransactionFlag?'o':'d')}{TransactionIdentifier})");
            if (WasRetransmitted)
                writer.Write(" Retransmit");
        }
    }

    /// <summary>
    /// catch and ignore frame ETSI EN 300 175-4 section 6.1.5
    /// </summary>
    public class InvalidProtocolDiscriminatorException : Exception
    {

    }
}