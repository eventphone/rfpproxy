﻿using System;
using System.Text;

namespace RfpProxyLib
{
    public static class Extensions
    {
        public static string CString(this ReadOnlySpan<byte> data)
        {
            var eos = data.IndexOf((byte) 0);
            if (eos < 0)
                return String.Empty;
            return Encoding.UTF8.GetString(data.Slice(0, eos));
        }

        public static bool IsEmpty(this ReadOnlySpan<byte> data)
        {
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] != 0) return false;
            }
            return true;
        }
    }
}