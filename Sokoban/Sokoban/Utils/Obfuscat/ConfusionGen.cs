using System.Security.Cryptography;

namespace Sokoban.Utils
{
    internal sealed class ConfusionGen
    {
        private readonly IdObfuscator obfuscator;
        private readonly HashAlgorithm sha256Algorithm;

        public ConfusionGen()
        {
            obfuscator = new();
            sha256Algorithm = SHA256.Create();
        }

        public string ConfuseBytes(byte[] rawData, bool useHash = true)
        {
            if (useHash)
            {
                return obfuscator.PermuteToBase62(sha256Algorithm.ComputeHash(rawData));
            }

            return obfuscator.PermuteToBase62(rawData);
        }
    }
}
