using System;
using System.Collections.Generic;
using System.Linq;

namespace IczpNet.Chat.TextTemplates
{
    /// <summary>
    /// 十进制转XX进制、XX进制转10进制【互转】
    /// </summary>
    public static class IntStringHelper
    {
        public const string StaticCode = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ_-";

        public static string IntToString(long v, int length = 36, string inText = StaticCode)
        {
            var arr = inText.Select(x => x).Take(length).ToArray();

            string ret = "";

            while (v >= 1)
            {
                int index = Convert.ToInt16(v - (v / length) * length);

                ret = arr[index] + ret;

                v /= length;
            }
            return ret;
        }

        public static long StringToInt(string v, int length = 36, string inText = StaticCode)
        {
            var arr = inText.Select(x => x).Take(length).ToArray();

            if (!v.All(arr.Contains))
            {
                throw new ArgumentException($"'{v}' is not existed:{arr.JoinAsString("")}");
            }

            long ret = 0;

            int power = v.Length - 1;

            for (int i = 0; i <= power; i++)
            {
                //ret += Array.IndexOf(arr, v[power - i]) * Convert.ToInt64(Math.Pow(length, i));
                ret += arr.FindIndex(x => x == v[power - i]) * Convert.ToInt64(Math.Pow(length, i));
            }
            return ret;
        }
    }
}
