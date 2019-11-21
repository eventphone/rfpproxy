using System;
using System.IO;
using RfpProxyLib;

namespace RfpProxy.AaMiDe.Rfpc
{
    public abstract class DnmRfpcValue
    {
        public RfpcKey Type { get; }

        public virtual bool HasUnknown => !Raw.IsEmpty;

        public virtual ReadOnlyMemory<byte> Raw { get; }

        protected DnmRfpcValue(RfpcKey type, ReadOnlyMemory<byte> data)
        {
            Type = type;
            Raw = data;
        }

        public virtual void Log(TextWriter writer)
        {
            writer.WriteLine();
            writer.Write($"\t{Type,-23}:");
            if (!Raw.IsEmpty)
            {
                if (HasUnknown)
                    writer.Write(" Reserved");
                else
                    writer.Write(" Padding");
                writer.Write($"({Raw.ToHex()})");
            }
        }

        public static DnmRfpcValue Create(RfpcKey type, ReadOnlyMemory<byte> data)
        {
            switch (type)
            {
                case RfpcKey.ExtendedCapabilities:
                    return new ExtendedCapabilitiesRfpcValue(data);
                case RfpcKey.ExtendedCapabilities2:
                    return new ExtendedCapabilities2RfpcValue(data);
                case RfpcKey.FrequencyBand:
                    return new ByteRfpcValue(RfpcKey.FrequencyBand, data);
                case RfpcKey.HigherLayerCapabilities:
                    return new HigherLayerCapabilitiesRfpcValue(data);
                case RfpcKey.MacCapabilities:
                    return new MacCapabilitiesRfpcValue(data);
                case RfpcKey.NumberOfUpn:
                    return new ByteRfpcValue(RfpcKey.NumberOfUpn, data);
                case RfpcKey.NumberOfBearer:
                    return new ByteRfpcValue(RfpcKey.NumberOfBearer, data);
                case RfpcKey.ReflectingEnvironment:
                    return new ReflectingEnvironmentRfpcValue(data);
                case RfpcKey.RfpFu6WindowSize:
                    return new ByteRfpcValue(RfpcKey.RfpFu6WindowSize, data);
                case RfpcKey.RFPI:
                    return new RfpiRfpcValue(data);
                case RfpcKey.RfpPli:
                    return new RfpPliRfpcValue(data);
                case RfpcKey.RfPower:
                    return new ByteRfpcValue(RfpcKey.RfPower, data);
                case RfpcKey.SARI:
                    return new SariRfpcValue(data);
                case RfpcKey.StatisticData:
                    return new StatisticDataRfpcValue(data);
                case RfpcKey.StatisticDataReset:
                    return new StatisticDataResetRfpcValue(data);
                case RfpcKey.Revision:
                    return new RevisionRfpcValue(data);
                case (RfpcKey)22:
                case RfpcKey.RfpSiteLocation:
                    return new UnknownDnmRfpcValue(type, data);//todo
                case RfpcKey.StatusInfo:
                case RfpcKey.ErrorCause:
                case RfpcKey.RfpToRfp:
                case RfpcKey.RfpTopo:
                case RfpcKey.LastError:
                case RfpcKey.PabxData:
                case RfpcKey.MoniData:
                case RfpcKey.LastErrorExt:
                case RfpcKey.FpgaRevision:
                case RfpcKey.RfpString:
                default:
                    return new UnknownDnmRfpcValue(type, data);
            }
        }
    }
}