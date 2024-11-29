using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Sokoban.Utils
{
    public static class ExtendMethod
    {
        [DllImport("Shlwapi.dll", CharSet = CharSet.Unicode)]
        public static extern int StrCmpLogicalW(string param1, string param2);

        private static readonly ConfusionGen _confusionGen;

        static ExtendMethod()
        {
            _confusionGen = new ConfusionGen();
        }

        /// <summary>
        /// 去除字节数组尾部的空白区(0x00)
        /// </summary>
        /// <param name="bytes">字节数组对象</param>
        public static byte[] BytesTrimEnd(this byte[] bytes)
        {
            List<byte> _list = bytes.ToList();

            for (int i = bytes.Length - 1; i >= 0; i--)
            {
                if (bytes[i] == 0x00)
                {
                    _list.RemoveAt(i);
                }
                else
                {
                    break;
                }
            }
            return _list.ToArray();
        }

        /// <summary>
        /// 对字节数组进行混淆化
        /// </summary>
        /// <param name="rawData">原始数据</param>
        /// <param name="useHash">指示是否对 <paramref name="rawData"/> 先进行哈希处理</param>
        /// <returns>
        /// 生成经过Base62编码的字符串
        /// </returns>
        public static string ConfuseBytes(byte[] rawData, bool useHash = true)
        {
            return _confusionGen.ConfuseBytes(rawData, useHash);
        }
    }
}
