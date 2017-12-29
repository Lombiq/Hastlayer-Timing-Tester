using System;
using System.IO;

namespace HastlayerTimingTester
{
    /// <summary>
    /// This is for easily printing formatted strings to log / screen.
    /// </summary>
    internal static class StreamWriterExtension
    {
        public static void FormattedWriteLine(this StreamWriter writer, string format, params Object[] args)
        {
            writer.WriteLine(string.Format(format, args));
        }
    }
}
