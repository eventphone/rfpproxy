using System;
using System.IO;

namespace RfpProxy.Log.Messages.Rfpc
{
    public abstract class DnmRfpcValue
    {
        public RfpcKey Type { get; }

        public abstract bool HasUnknown { get; }

        protected DnmRfpcValue(RfpcKey type)
        {
            Type = type;
        }

        public virtual void Log(TextWriter writer)
        {
            writer.WriteLine();
            writer.Write($"\t{Type,-23}:");
        }

        public static DnmRfpcValue Create(RfpcKey type, ReadOnlyMemory<byte> data)
        {
            switch (type)
            {
                case RfpcKey.ExtendedCapabilities:
                    return new ExtendedCapabilitiesRfpcValue(data);
                case RfpcKey.ExtendedCapabilities2:
                    return new ExtendedCapabilities2RfpcValue(data);
                case RfpcKey.HigherLayerCapabilities:
                    return new HigherLayerCapabilitiesRfpcValue(data);
                case RfpcKey.MacCapabilities:
                    return new MacCapabilitiesRfpcValue(data);
                case RfpcKey.NumberOfUpn:
                    return new NumberOfUpnRfpcValue(data);
                case RfpcKey.NumberOfBearer:
                    return new NumberOfBearerRfpcValue(data);
                case RfpcKey.ReflectingEnvironment:
                    return new ReflectingEnvironmentRfpcValue(data);
                case RfpcKey.RFPI:
                    return new RfpiRfpcValue(data);
                case RfpcKey.RfpPli:
                    return new RfpPliRfpcValue(data);
                case RfpcKey.SARI:
                    return new SariRfpcValue(data);
                case RfpcKey.StatisticData:
                    return new StatisticDataRfpcValue(data);
                case RfpcKey.StatisticDataReset:
                    return new StatisticDataResetRfpcValue(data);
                case RfpcKey.Revision:
                case RfpcKey.StatusInfo:
                case RfpcKey.ErrorCause:
                case RfpcKey.RfpFu6WindowSize:
                case RfpcKey.RfpToRfp:
                case RfpcKey.RfpTopo:
                case RfpcKey.LastError:
                case RfpcKey.PabxData:
                case RfpcKey.MoniData:
                case RfpcKey.LastErrorExt:
                case RfpcKey.FpgaRevision:
                case RfpcKey.RfpString:
                case RfpcKey.RfpSiteLocation:
                case RfpcKey.FrequencyBand:
                case RfpcKey.RfPower:
                default:
                    return new UnknownDnmRfpcValue(type, data);
            }
        }
    }
}