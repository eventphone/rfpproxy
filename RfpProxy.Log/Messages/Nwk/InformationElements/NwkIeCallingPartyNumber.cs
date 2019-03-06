using System;
using System.IO;
using System.Text;
using RfpProxyLib;

namespace RfpProxy.Log.Messages.Nwk.InformationElements
{
    public sealed class NwkIeCallingPartyNumber : NwkVariableLengthInformationElement
    {
        public enum NumberTypeCoding : byte
        {
            Unknown=0b0000,
            InternationalNumber=0b0001,
            NationalNumber=0b0010,
            NetworkSpecificNumber=0b0011,
            SubscriberNumber=0b0100,
            AbbreviatedNumber=0b0110,
            ReservedForExtension=0b0111,
        }

        public enum NumberingPlanCoding : byte
        {
            Unknown=0b0000,
            ISDN=0b0001,
            DataPlan=0b0011,
            TcpIp=0b0111,
            NationalStandardPlan=0b1000,
            PrivatePlan=0b1001,
            SIP=0b1010,
            InternetCharacterFormat=0b1011,
            MAC=0b1100,
            X400=0b1101,
            ProfileServiceSpecificAlphanumericIdentifier=0b1110,
        }

        public enum PresentIndicator : byte
        {
            PresentationAllowed = 0b0000,
            PresentationRestricted = 0b0001,
            NumberNotAvailable = 0b0010,
        }

        public enum ScreeningIndicator : byte
        {
            UserUnscreened = 0b0000,
            UserVerified = 0b0001,
            UserVerificationFailed = 0b0010,
            NetworkProvided = 0b0011
        }

        public NumberTypeCoding NumberType { get; }

        public NumberingPlanCoding NumberingPlan { get; }

        public bool Has3a { get; }

        public PresentIndicator Presentation { get; }

        public ScreeningIndicator Screening { get; }

        public ReadOnlyMemory<byte> Number { get; }

        public override bool HasUnknown => false;

        public NwkIeCallingPartyNumber(ReadOnlyMemory<byte> data):base(NwkVariableLengthElementType.CallingPartyNumber)
        {
            var span = data.Span;
            NumberType = (NumberTypeCoding) ((span[0] & 0x70) >> 4);
            NumberingPlan = (NumberingPlanCoding) (span[0] & 0x0f);
            if (span[0] < 128)
            {
                Has3a = true;
                Presentation = (PresentIndicator) ((span[1] & 0x60) >> 5);
                Screening = (ScreeningIndicator) (span[1] & 0x03);
                Number = data.Slice(2);
            }
            else
            {
                Number = data.Slice(1);
            }

        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($" NumberType({NumberType}) NumberingPlan({NumberingPlan})");
            if (Has3a)
                writer.Write($" Presentation({Presentation:G}) Screening({Screening:G})");
            writer.Write($" Address({Encoding.ASCII.GetString(Number.Span)})");
        }
    }
}