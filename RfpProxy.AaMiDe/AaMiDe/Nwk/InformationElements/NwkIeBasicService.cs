using System;
using System.IO;

namespace RfpProxy.AaMiDe.Nwk.InformationElements
{
    public sealed class NwkIeBasicService : NwkDoubleByteInformationElement
    {
        public enum CallClassType : byte
        {
            LiAServiceSetup = 0b0010,
            MessageCallSetup = 0b0100,
            DectIsdnIIP = 0b0111,
            NormalCallSetup = 0b1000,
            InternalCallSetup = 0b1001,
            EmergencyCallSetup = 0b1010,
            ServiceCallSetup = 0b1011,
            ExternalHandoverCallSetup = 0b1100,
            SupplementaryServiceCallSetup = 0b1101,
            OAMCallSetup = 0b1110,
        }

        public enum BasicServiceType
        {
            BasicSpeech = 0b0000,
            DectGsmIwpProfile = 0b0100,
            DectUmtsIwp = 0b0110,
            GsmIwpSms = 0b0110,
            LrmsService = 0b0101,
            WidebandSpeech = 0b1000,
            LightDataServicesSuotaClass4 = 0b1001,
            LightDataServicesSuotaClass3 = 0b1010,
            Other = 0b1111,
        }

        public CallClassType CallClass { get; }

        public BasicServiceType BasicService { get; }

        public override bool HasUnknown => !Enum.IsDefined(typeof(CallClassType), CallClass) || !Enum.IsDefined(typeof(BasicServiceType), BasicService);

        public NwkIeBasicService(byte content) : base(NwkDoubleByteElementType.BasicService)
        {
            CallClass = (CallClassType) (content >> 4);
            BasicService = (BasicServiceType) (content & 0xf);
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($" CallClass({CallClass}) BasicService({BasicService})");
        }
    }
}