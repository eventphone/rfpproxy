using System;

namespace RfpProxy
{
    public static class StdLib
    {
        public static uint RandR(uint seed)
        {
            return RandR(ref seed);
        }

        public static uint RandR(ref uint seed)
        {
            uint result;
            seed *= 1103515245;
            seed += 12345;
            result = seed / 65536 % 2048;
            seed *= 1103515245;
            seed += 12345;
            result <<= 10;
            result ^= (seed / 65536) % 1024;
            seed *= 1103515245;
            seed += 12345;
            result <<= 10;
            result ^= (seed / 65536) % 1024;
            return result;
        }
    }
}