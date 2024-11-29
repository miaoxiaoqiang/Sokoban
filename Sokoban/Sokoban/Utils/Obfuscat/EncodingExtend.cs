using System;
using System.Collections.Generic;
using System.Text;

namespace Sokoban.Utils
{
    internal static class EncodingExtend
    {
        private const string DefaultCharacterSet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
        private const string InvertedCharacterSet = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

        public static string ToBase62(this ulong original, bool inverted = false)
        {
            byte[] array = BitConverter.GetBytes(original);

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(array);
            }

            return array.ToBase62(inverted);
        }

        public static string ToBase62(this byte[] original, bool inverted = false)
        {
            var characterSet = inverted ? InvertedCharacterSet : DefaultCharacterSet;
            var arr = Array.ConvertAll(original, t => (int)t);

            var converted = BaseConvert(arr, 256, 62);
            var builder = new StringBuilder();
            foreach (var t in converted)
            {
                builder.Append(characterSet[t]);
            }
            return builder.ToString();
        }

        private static int[] BaseConvert(int[] source, int sourceBase, int targetBase)
        {
            var result = new List<int>();
            int count;

            while ((count = source.Length) > 0)
            {
                var quotient = new List<int>();
                var remainder = 0;

                for (var i = 0; i != count; i++)
                {
                    var accumulator = source[i] + remainder * sourceBase;
                    var digit = accumulator / targetBase;
                    remainder = accumulator % targetBase;

                    if (quotient.Count > 0 || digit > 0)
                    {
                        quotient.Add(digit);
                    }
                }

                result.Insert(0, remainder);
                source = quotient.ToArray();
            }

            return result.ToArray();
        }
    }
}
