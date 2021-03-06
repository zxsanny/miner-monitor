﻿using System.Linq;

namespace HashHunters.MinerMonitor.Extensions
{
    public static class StringExtensions
    {
        public static string TrimAll(this string input)
        {
            return new string(input.ToCharArray().Where(c => !char.IsWhiteSpace(c)).ToArray());
        }
    }
}
