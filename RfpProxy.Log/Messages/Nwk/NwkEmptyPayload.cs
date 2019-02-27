﻿using System.IO;

namespace RfpProxy.Log.Messages.Nwk
{
    public class NwkEmptyPayload : NwkPayload
    {
        public override bool HasUnknown => false;

        public NwkEmptyPayload() : base(NwkProtocolDiscriminator.CISS, 0, false)
        {
        }

        public override void Log(TextWriter writer)
        {
        }
    }
}