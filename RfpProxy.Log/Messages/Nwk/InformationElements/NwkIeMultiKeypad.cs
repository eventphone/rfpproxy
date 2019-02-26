using System;
using System.IO;

namespace RfpProxy.Log.Messages.Nwk.InformationElements
{
    public sealed class NwkIeMultiKeypad : NwkVariableLengthInformationElement
    {
        public enum ControlCodes : byte
        {
            Null = 0x00,
            ReturnHome = 0x02,
            ReturnEnd = 0x03,
            DiallingPause = 0x05,
            MoveForwardToNextColumn = 0x06,
            MoveBackwardToNextColumn = 0x07,
            MoveBackwardOneColumn = 0x08,
            MoveForwardOneColumn = 0x09,
            MoveDownOneRow = 0x0a,
            MoveUpOneRow = 0x0b,
            ClearDisplayAndReturnHome = 0x0c,
            ReturnToStartOfCurrentRow = 0x0d,
            FlashOff = 0x0e,
            FlashOn = 0x0f,
            Xon = 0x11,
            GoToPulseDialling = 0x12,
            Xoff = 0x13,
            GoToDtmfDiallingDefinedToneLength = 0x14,
            RegisterRecall = 0x15,
            GoToDtmfDiallingInfiniteToneLength = 0x16,
            InternalCall = 0x17,
            ServiceCall = 0x18,
            ClearToEndOfDisplay = 0x19,
            ClearToEndOfLine = 0x1a,
            Esc = 0x1b,
            SupplementaryService = 0x1c,
        }

        public override bool HasUnknown { get; }

        public ReadOnlyMemory<byte> Keys { get; }

        public NwkIeMultiKeypad(ReadOnlyMemory<byte> data) : base(NwkVariableLengthElementType.MultiKeypad)
        {
            Keys = data;
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            if (Keys.IsEmpty) return;
            writer.Write(" Keys(");
            foreach (var key in Keys.Span)
            {
                if (key < 0x1f)
                {
                    writer.Write(((ControlCodes)key).ToString("G"));
                }
                else if (key > 128)
                {
                    writer.Write(key.ToString("x2"));
                }
                else
                {
                    writer.Write((char)key);
                }
                writer.Write(' ');
            }
            writer.Write(")");
        }
    }
}