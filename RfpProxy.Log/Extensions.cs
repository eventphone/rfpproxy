using System;
using System.Text;

namespace RfpProxy.Log
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
    }
}