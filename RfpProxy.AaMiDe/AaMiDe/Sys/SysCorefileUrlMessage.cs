﻿using System;
using System.IO;
using RfpProxyLib;

namespace RfpProxy.AaMiDe.Sys
{
    public sealed class SysCorefileUrlMessage : AaMiDeMessage
    {
        public string Url { get; }

        public override bool HasUnknown => false;

        protected override ReadOnlyMemory<byte> Raw => ReadOnlyMemory<byte>.Empty;

        public SysCorefileUrlMessage(ReadOnlyMemory<byte> data):base(MsgType.SYS_CORE_DUMP, data)
        {
            Url = base.Raw.Span.CString();
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($"Url({Url})");
        }
    }
}