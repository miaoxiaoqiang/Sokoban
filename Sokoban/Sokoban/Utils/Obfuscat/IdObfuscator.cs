using System;

namespace Sokoban.Utils
{
    internal sealed class IdObfuscator
    {
        private readonly Feistel feistel;

        public IdObfuscator()
        {
            feistel = new Feistel();
            //base62Transfer = new Base62();
        }

        public ulong Permute(ulong id)
        {
            return feistel.Permute(id);
        }

        public string PermuteToBase62(ulong id)
        {
            return Permute(id).ToBase62();
        }

        public string PermuteToBase62(byte[] data)
        {
            if (null == data)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (data.Length >= sizeof(ulong))
            {
                return Permute(BitConverter.ToUInt64(data, 0)).ToBase62();
            }

            byte[] complete = new byte[sizeof(ulong)];

            for (int i = 0; i < data.Length; ++i)
            {
                if (BitConverter.IsLittleEndian) // we have two ways of padding
                {
                    complete[i] = data[i];
                }
                else
                {
                    complete[complete.Length - i - 1] = data[data.Length - i - 1];
                }
            }

            return Permute(BitConverter.ToUInt64(complete, 0)).ToBase62();
        }
    }
}
