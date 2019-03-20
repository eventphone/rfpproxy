using System;
using System.IO;

namespace RfpProxy.Log.Messages.Nwk.InformationElements
{
    public sealed class NwkIeSignal : NwkDoubleByteInformationElement
    {
        public enum SignalCoding : byte
        {
            DialToneOn = 0b0000_0000,
            RingBackToneOn = 0b0000_0001,
            InterceptToneOn = 0b0000_0010,
            NetworkCongestionToneOn = 0b0000_0011,
            BusyToneOn = 0b0000_0100,
            ConfirmToneOn = 0b0000_0101,
            AnswerToneOn = 0b0000_0110,
            CallWaitingToneOn = 0b0000_0111,
            OffHookWarningToneOn = 0b0000_1000,
            NegativeAcknowledgementTone = 0b0000_1001,
            TonesOff = 0b0011_1111,
            AlertingOnPattern0 = 0b0100_0000,
            AlertingOnPattern1 = 0b0100_0001,
            AlertingOnPattern2 = 0b0100_0010,
            AlertingOnPattern3 = 0b0100_0011,
            AlertingOnPattern4 = 0b0100_0100,
            AlertingOnPattern5 = 0b0100_0101,
            AlertingOnPattern6 = 0b0100_0110,
            AlertingOnPattern7 = 0b0100_0111,
            AlertingOnContinuous = 0b0100_1000,
            AlertingOff = 0b0100_1111,
        }

        public SignalCoding Signal { get; }

        public override bool HasUnknown => !Enum.IsDefined(typeof(SignalCoding), Signal);

        public NwkIeSignal(byte content):base(NwkDoubleByteElementType.Signal)
        {
            Signal = (SignalCoding) content;
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($" Signal({Signal})");
        }
    }
}