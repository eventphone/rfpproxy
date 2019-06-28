using System;
using System.Buffers.Binary;
using System.IO;
using RfpProxy.Log.Messages.Nwk.InformationElements.Proprietary;
using RfpProxyLib;

namespace RfpProxy.Log.Messages.Nwk.InformationElements
{
    public sealed class NwkIeIwu2Iwu : NwkVariableLengthInformationElement
    {
        public enum ProtocolDiscriminatorType : byte
        {
            UserSpecific = 0b0000_0000,
            OsiHighLayerProtocols = 0b0000_0001,
            X263 = 0b0000_0010,
            ListAccess = 0b0000_0011,
            Ia5Characters = 0b0000_0100,
            LightDataService = 0b0000_0110,
            Suota = 0b0000_0110,
            V120 = 0b0000_0111,
            Q931Message = 0b0000_1000,
            Q931InformationElement = 0b0000_1001,
            Q931PartialMessage = 0b0000_1010,
            GsmMessage = 0b0001_0000,
            GsmInformationElement = 0b0001_0001,
            UmtsGprsInformationElement = 0b0001_0010,
            UmtsGprsMessages = 0b0001_0011,
            Lrms = 0b0001_0100,
            RllAccessProfile = 0b0001_0101,
            Wrs = 0b0001_0110,
            DectIsdnCPlane = 0b0010_0000,
            DectIsdnUPlane = 0b0010_0001,
            DectIsdnOperationAndMaintenance = 0b0010_0010,
            TerminalData = 0b0010_0011,
            DectAccessToIpNetworks = 0b0010_0100,
            Mpeg4 = 0b0010_0101,
            Unknown = 0b0011_1111,
        }

        public enum DiscriminatorType : byte
        {
            Unspecified = 0b1000_0000,
            EMC = 0b1000_0001,
        }

        /// <summary>
        /// Send/Reject (S/R) bit:
        /// </summary>
        public bool Send { get; }

        public ProtocolDiscriminatorType ProtocolDiscriminator { get; }

        public DiscriminatorType Discriminator { get; }

        public ushort EMC { get; }

        public NwkIeProprietaryContent Content { get; }

        public override ReadOnlyMemory<byte> Raw { get; }

        public override bool HasUnknown => ProtocolDiscriminator != ProtocolDiscriminatorType.UserSpecific ||
                                           Discriminator != DiscriminatorType.EMC ||
                                           Content.HasUnknown;

        public NwkIeIwu2Iwu(ReadOnlyMemory<byte> data):base(NwkVariableLengthElementType.IWU2IWU, data)
        {
            var span = data.Span;
            Send = (span[0] & 0x40) != 0;
            ProtocolDiscriminator = (ProtocolDiscriminatorType) (span[0] & 0x3f);
            if (ProtocolDiscriminator == ProtocolDiscriminatorType.UserSpecific)
            {
                Discriminator = (DiscriminatorType) span[1];
                if (Discriminator == DiscriminatorType.EMC)
                {
                    EMC = BinaryPrimitives.ReadUInt16BigEndian(data.Span.Slice(2));
                    Raw = ReadOnlyMemory<byte>.Empty;
                    Content = NwkIeProprietaryContent.Create(EMC, data.Slice(4));
                }
                else
                {
                    Raw = data.Slice(2);
                }
            }
            else
            {
                Raw = data.Slice(1);
            }
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($" {(Send?"Send":"Reject")}({ProtocolDiscriminator:G})");
            if (ProtocolDiscriminator == ProtocolDiscriminatorType.UserSpecific)
            {
                if (Discriminator == DiscriminatorType.EMC)
                {
                    writer.Write($" EMC({EMC:x4})");
                    Content.Log(writer);
                }
                else
                {
                    writer.Write($" Unspecified({Raw.ToHex()})");
                }
            }
            else
            {
                writer.Write($" Reserved({Raw.ToHex()})");
            }
        }
    }
}