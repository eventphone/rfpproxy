using System;

namespace RfpProxyLib.AaMiDe.Sys
{
    public sealed class SysOmmControlMessage : AaMiDeMessage
    {
        public override bool HasUnknown => Raw.Length > 0;

        public SysOmmControlMessage(ReadOnlyMemory<byte> data):base(MsgType.SYS_OMM_CONTROL, data)
        {
        }
    }
}