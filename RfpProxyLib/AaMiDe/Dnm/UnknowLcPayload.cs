﻿using System;
using System.IO;

namespace RfpProxyLib.AaMiDe.Dnm
{
    public sealed class UnknowLcPayload : LcPayload
    {
        public override bool HasUnknown => true;

        public UnknowLcPayload(ReadOnlyMemory<byte> data) : base(data)
        {
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            if (Length > 0)
            {
                writer.Write($" Reserved2({Raw.ToHex()})");
            }
        }
    }
}