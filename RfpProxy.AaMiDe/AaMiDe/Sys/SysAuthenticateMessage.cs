﻿using System;
using System.IO;
using RfpProxyLib;

namespace RfpProxy.AaMiDe.Sys
{
    public sealed class SysAuthenticateMessage : AaMiDeMessage
    {
        public ReadOnlyMemory<byte> Reserved1 { get; }

        public ReadOnlyMemory<byte> RfpIv { get; }
        
        public ReadOnlyMemory<byte> Reserved2 { get; }

        public ReadOnlyMemory<byte> OmmIv { get; }

        public override bool HasUnknown => true;

        protected override ReadOnlyMemory<byte> Raw => base.Raw.Slice(31);

        public SysAuthenticateMessage(ReadOnlyMemory<byte> data):base(MsgType.SYS_AUTHENTICATE, data)
        {
            Reserved1 = base.Raw.Slice(0, 7);
            RfpIv = base.Raw.Slice(7, 8);
            Reserved2 = base.Raw.Slice(15, 8);
            OmmIv = base.Raw.Slice(23, 8);
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($"Reserved1({Reserved1.ToHex()}) ");
            writer.Write($"RfpIv({RfpIv.ToHex()}) ");
            writer.Write($"Reserved2({Reserved2.ToHex()}) ");
            writer.Write($"OmmIv({OmmIv.ToHex()})");
        }
    }
}