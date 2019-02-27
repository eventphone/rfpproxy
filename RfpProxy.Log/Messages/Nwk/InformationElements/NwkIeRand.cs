﻿using System;
using System.IO;
using RfpProxyLib;

namespace RfpProxy.Log.Messages.Nwk.InformationElements
{
    public sealed class NwkIeRand : NwkVariableLengthInformationElement
    {
        public ReadOnlyMemory<byte> Rand { get; }

        public override bool HasUnknown => false;

        public NwkIeRand(ReadOnlyMemory<byte> data) : base(NwkVariableLengthElementType.RAND)
        {
            Rand = data;
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($" RAND({Rand.ToHex()})");
        }
    }
}