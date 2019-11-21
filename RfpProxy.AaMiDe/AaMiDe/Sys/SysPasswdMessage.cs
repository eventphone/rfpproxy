using System;
using System.IO;
using RfpProxyLib;

namespace RfpProxy.AaMiDe.Sys
{
    public sealed class SysPasswdMessage : AaMiDeMessage
    {
        public bool IsRemoteAccessEnabled { get; }

        public byte Padding1 { get; }

        public string RootUser { get; }

        public string RootPassword { get; }

        public string AdminUser { get; }

        public string AdminPassword { get; }

        /// <summary>
        /// padding
        /// </summary>
        protected override ReadOnlyMemory<byte> Raw => base.Raw.Slice(0x106);

        public override bool HasUnknown => false;

        public SysPasswdMessage(ReadOnlyMemory<byte> data):base(MsgType.SYS_PASSWD, data)
        {
            var span = base.Raw.Span;
            IsRemoteAccessEnabled = (span[0] & 1) != 0;
            Padding1 = span[1];
            RootUser = span.Slice(2, 0x41).CString();
            RootPassword = span.Slice(0x43, 0x41).CString();
            AdminUser = span.Slice(0x84, 0x41).CString();
            AdminPassword = span.Slice(0xc5, 0x41).CString();
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($"Padding1({Padding1:x2}) ");
            writer.Write($"RemoteAccess({IsRemoteAccessEnabled}) ");
            writer.Write($"RootUser({RootUser}) RootPass({RootPassword}) ");
            writer.Write($"AdminUser({AdminUser}) AdminPass({AdminPassword})");
        }
    }
}