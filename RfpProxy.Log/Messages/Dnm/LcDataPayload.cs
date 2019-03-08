using System;
using System.IO;
using RfpProxy.Log.Messages.Nwk;
using RfpProxyLib;

namespace RfpProxy.Log.Messages.Dnm
{
    public enum LcCommandType
    {
        Unknown,
        /// <summary>
        /// Numbered information
        /// </summary>
        I,
        /// <summary>
        /// Receive ready
        /// </summary>
        RR,
        /// <summary>
        /// Receive not ready
        /// </summary>
        RNR,
        /// <summary>
        /// Reject
        /// </summary>
        REJ,
        /// <summary>
        /// Set Async Bal Mode
        /// </summary>
        SABM,
        /// <summary>
        /// Disconnect mode
        /// </summary>
        DM,
        /// <summary>
        /// Unnumbered information
        /// </summary>
        UI,
        /// <summary>
        /// Disconnect
        /// </summary>
        DISC,
        /// <summary>
        /// Unnumbered ack
        /// </summary>
        UA
    }

    public sealed class LcDataPayload : LcPayload
    {
        /// <summary>
        /// The C/R bit shall identify a frame as either a command or a response.
        /// The PT side shall send commands with C/R set to "0" and responses with C/R set to "1".
        /// The FT side shall do the opposite.
        /// </summary>
        public bool Command { get; }

        /// <summary>
        /// The SAPI shall identify the higher layer SAP that is associated with each frame.
        /// SAPI = "0": connection oriented signalling
        /// SAPI = "3": connectionless signalling
        /// </summary>
        public byte SAPI { get; }

        /// <summary>
        /// The LLN shall identify the LAPC entity that is associated with each frame.
        /// </summary>
        public byte LLN { get; }

        /// <summary>
        /// New Link Flag
        /// </summary>
        public bool NLF { get; }

        /// <summary>
        /// Send sequence Number
        /// </summary>
        public byte Ns { get; }

        /// <summary>
        /// Receive sequence Number
        /// </summary>
        public byte Nr { get; }
        /// <summary>
        /// Poll/Final bit
        /// Poll when issued as a command
        /// Final when issued as a response
        /// </summary>
        public bool PollFinal { get; }

        public LcCommandType CommandType { get; }

        public bool ExtendedLength { get; }

        public bool MoreData { get; }

        public byte PayloadLength { get; }

        public NwkPayload Payload { get; }

        public override bool HasUnknown => Payload.HasUnknown;

        public LcDataPayload(ReadOnlyMemory<byte> data, NwkReassembler reassembler):base(data)
        {
            var span = base.Raw.Span;
            Command = (span[0] & 0x2) == 0x2;
            SAPI = (byte) ((span[0] & 0xc) >> 2);
            LLN = (byte) ((span[0] & 0x70) >> 4);

            var control = span[1];
            //ETSI EN 300 175-4 7.11
            if ((control & 1) == 0)
            {
                CommandType = LcCommandType.I;
                Ns = (byte) ((control & 0xe) >> 1);
                PollFinal = (control & 0x10) != 0;
                Nr = (byte) (control >> 5);
            }
            else
            {
                Ns = (byte) ((control & 0xe) >> 1);
                PollFinal = (control & 0x10) != 0;
                Nr = (byte) (control >> 5);
                if (Ns == 0)
                {
                    CommandType = LcCommandType.RR;
                }
                else if (Ns == 2)
                {
                    Ns = 0;
                    CommandType = LcCommandType.RNR;
                }
                else if (Ns == 4)
                {
                    Ns = 0;
                    CommandType = LcCommandType.REJ;
                }
                else if (Ns == 7)
                {
                    if (Nr == 1)
                    {
                        Ns = 0;
                        Nr = 0;
                        CommandType = LcCommandType.SABM;
                    }
                    else if (Nr == 0)
                    {
                        Ns = 0;
                        Nr = 0;
                        CommandType = LcCommandType.DM;
                    }
                }
                else if (Ns == 1)
                {
                    if (Nr == 0)
                    {
                        Ns = 0;
                        Nr = 0;
                        CommandType = LcCommandType.UI;
                    }
                    else if (Nr == 2)
                    {
                        Ns = 0;
                        Nr = 0;
                        CommandType = LcCommandType.DISC;
                    }
                    else if (Nr == 3)
                    {
                        Ns = 0;
                        Nr = 0;
                        CommandType = LcCommandType.UA;
                    }
                }
            }

            ExtendedLength = (span[2] & 0x1) != 1;
            MoreData = (span[2] & 0x2) == 0x2;
            ReadOnlyMemory<byte> payloadData;
            if (!ExtendedLength)
            {
                PayloadLength = (byte) (span[2] >> 2);
                payloadData = base.Raw.Slice(3, PayloadLength);
            }
            else
            {
                payloadData = base.Raw.Slice(4);
            }
            var lln = LLN;
            if (Command)
                lln |= 0b1000;
            if (MoreData)
            {
                reassembler.AddFragment(lln, payloadData);
                Payload = new NwkFragmentedPayload(payloadData);
            }
            else
            {
                payloadData = reassembler.Reassemble(lln, payloadData);
                Payload = NwkPayload.Create(payloadData);
            }
        }

        public override ReadOnlyMemory<byte> Raw
        {
            get
            {
                if (ExtendedLength)
                    return base.Raw.Slice(4);
                return base.Raw.Slice(3).Slice(PayloadLength);
            }
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($" Command({(Command?0:1)}) SAPI({SAPI}) LLN({LLN}) NLF({NLF})");
            writer.Write($" Type({CommandType})");
            switch (CommandType)
            {
                case LcCommandType.Unknown:
                case LcCommandType.I:
                    writer.Write($" Ns({Ns}) P/F({PollFinal}) Nr({Nr})");
                    break;
                case LcCommandType.RR:
                case LcCommandType.RNR:
                case LcCommandType.REJ:
                    writer.Write($" P/F({PollFinal}) Nr({Nr})");
                    break;
                case LcCommandType.SABM:
                case LcCommandType.DM:
                case LcCommandType.UI:
                case LcCommandType.DISC:
                case LcCommandType.UA:
                    writer.Write($" P/F({PollFinal})");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            writer.Write($" N({(ExtendedLength?0:1)}) M({(MoreData?1:0)}) L({PayloadLength})");
            if (!Raw.IsEmpty)
                writer.Write($" Reserved({Raw.ToHex()})");

            Payload.Log(writer);
        }
    }
}