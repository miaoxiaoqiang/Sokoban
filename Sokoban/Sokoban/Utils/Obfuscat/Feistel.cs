using System;

namespace Sokoban.Utils
{
    internal sealed class Feistel
    {
        private double RoundFunction(ulong input)
        {
            return ((1369 * input + 150889) % 714025) / 714025.0;
        }

        public ulong Permute(ulong n)
        {
            ulong l1 = (n >> 32) & 4294967295L;
            ulong r1 = n & 4294967295L;
            ulong l2, r2;
            for (int i = 0; i < 3; i++)
            {
                l2 = r1;
                r2 = l1 ^ (ulong)(this.RoundFunction(r1) * 4294967295L);
                l1 = l2;
                r1 = r2;
            }
            return ((r1 << 32) + l1);
        }

        public uint Permute(uint n)
        {
            uint l1 = (n >> 16) & 65535;
            uint r1 = n & 65535;
            uint l2, r2;
            for (int i = 0; i < 3; i++)
            {
                l2 = r1;
                r2 = l1 ^ (uint)(RoundFunction(r1) * 65535);
                l1 = l2;
                r1 = r2;
            }
            return ((r1 << 16) + l1);
        }
    }
}
