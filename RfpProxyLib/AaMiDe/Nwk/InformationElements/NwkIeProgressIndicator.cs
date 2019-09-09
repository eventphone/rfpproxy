using System;
using System.IO;

namespace RfpProxyLib.AaMiDe.Nwk.InformationElements
{
    public sealed class NwkIeProgressIndicator : NwkVariableLengthInformationElement
    {
        public enum CodingStandard:byte
        {
            ItuT = 0,
            Reserved = 1,
            National = 2,
            Location = 3
        }

        public enum LocationCoding:byte
        {
            User = 0b0000,
            PrivateNetworkLocalUser = 0b0001,
            PublicNetworkLocalUser = 0b0010,
            PublicNetworkRemoteUser = 0b0100,
            PrivateNetworkRemoteUser = 0b0101,
            InternationalNetwork = 0b0111,
            BeyondInternetworkingPoint = 0b1010,
            NotApplicable = 0b1111
        }

        public enum ProgressDescriptionCoding : byte
        {
            NotEndToEndIsdn=0b0000001,
            DestinationNonIsdn=0b0000010,
            OriginationNonIsdn=0b0000011,
            ReturnedToTheIsdn=0b0000100,
            ServiceChangeOccurred=0b0000101,
            InBandInformationOrAppropriatePatternNowAvailable=0b0001000,
            InBandInformationNotAvailable=0b0001001,
            CallIsEndToEndIsdn=0b0100000,
        }

        public CodingStandard Coding { get; }

        public LocationCoding Location { get; }

        public ProgressDescriptionCoding Description { get; }

        public override bool HasUnknown => false;

        public NwkIeProgressIndicator(ReadOnlyMemory<byte> data):base(NwkVariableLengthElementType.ProgressIndicator, data)
        {
            Coding = (CodingStandard) ((data.Span[0] >> 4) & 0x3);
            Location = (LocationCoding) (data.Span[0] & 0xf);
            Description = (ProgressDescriptionCoding) (data.Span[1] & 0x7f);
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($" Coding({Coding}) Location({Location}) Description({Description})");
        }
    }
}