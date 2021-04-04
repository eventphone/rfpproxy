using System;

namespace RfpProxy.AaMiDe.Sys
{
    public sealed class SysEncryptionConf : AaMiDeMessage
    {
        public SysEncryptionConf() : base(MsgType.SYS_ENCRYPTION_CONF)
        {
        }

        public SysEncryptionConf(ReadOnlyMemory<byte> data) : base(MsgType.SYS_ENCRYPTION_CONF, data)
        {
        }
    }
}