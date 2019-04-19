using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SerialHandler.Demo
{
    internal static class BytesHelper
    {
        /// <summary>
        /// Connect all bytes in parameter order.
        /// </summary>
        /// <param name="buff1">first bytes</param>
        /// <param name="buff2">second bytes</param>
        /// <param name="buffList">other bytes</param>
        /// <returns></returns>
        public static byte[] Concat(byte[] buff1, byte[] buff2, params byte[][] buffList)
        {
            List<byte> list = new List<byte>();
            foreach (byte b in buff1)
                list.Add(b);
            foreach (byte b in buff2)
                list.Add(b);
            for (int i = 0; i < buffList.Length; i++)
            {
                for (int j = 0; j < buffList[i].Length; j++)
                    list.Add(buffList[i][j]);
            }
            return list.ToArray();
        }
    }
}
