﻿using System;
using System.IO;

namespace RfpProxy.AaMiDe.Nwk
{
    public class NwkFragmentedPayload : NwkPayload
    {
        public override bool HasUnknown => false;

        public ReadOnlyMemory<byte> Fragment { get; }

        public NwkFragmentedPayload(ReadOnlyMemory<byte> data) : base(NwkProtocolDiscriminator.MM, 0, false)
        {
            Fragment = data;
        }

        public override void Log(TextWriter writer)
        {
            writer.WriteLine();
            writer.Write("\tFragmented");
            if (WasRetransmitted)
                writer.Write(" Retransmit");
        }
    }
}