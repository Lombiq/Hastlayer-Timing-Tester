using System;
using System.Globalization;
using System.IO;

namespace HastlayerTimingTester
{
    /// <summary>
    /// Logger writes a formatted string to both a log file (Log.txt in CurrentTestOutputBaseDirectory) and the
    /// console. It also handles writing to the results file (Results.tsv in CurrentTestOutputBaseDirectory).
    /// </summary>
    internal static class Logger
    {
        private static StreamWriter _logStreamWriter;
        private static bool _initialized;


        /// <summary>
        /// Initializes the Logger, to open the file given in LogFilePath. (Logger already works before initialization,
        /// but it only writes to the console.)
        /// </summary>
        public static void Init(string logFilePath, bool createFile = true)
        {
            _logStreamWriter = new StreamWriter(File.Open(logFilePath, createFile ? FileMode.Create : FileMode.Append))
            {
                AutoFlush = true,
            };
            _initialized = true;
        }

        /// <summary>
        /// Writes a formatted string to both a log file (if already initialized) and the console, ending with a line
        /// break.
        /// </summary>
        public static void Log(string format, params object[] objs) => LogInternal(format, false, objs);

        /// <summary>
        /// Writes a formatted string to both a log file (if already initialized) and the console. It does not end with
        /// a line break.
        /// </summary>
        public static void LogInline(string format, params object[] objs) => LogInternal(format, true, objs);

        /// <summary>
        /// Writes a header to the log and screen for a given processing stage of Hastlayer Timing Tester, which can be
        /// specified as an input parameter (stage).
        /// </summary>
        public static void LogStageHeader(string stage)
        {
            Log(Environment.NewLine + "=== HastlayerTimingTester {0} stage ===", stage);
            Log("Started at {0}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Writes a footer to the log and screen for a given processing stage of Hastlayer Timing Tester, which can be
        /// specified as an input parameter (stage).
        /// </summary>
        public static void LogStageFooter(string stage) =>
            Log(
                Environment.NewLine + "=== HastlayerTimingTester {0} stage finished at {1} ===",
                stage,
                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));

        public static void Close()
        {
            _logStreamWriter?.Dispose();
            _initialized = false;
        }


        /// <summary>
        /// Implements the functionality described for <see cref="Log"/> and <see cref="LogInline"/>.
        /// </summary>
        /// <param name="inline">It ends the line with a line break based on this parameter.</param>
        private static void LogInternal(string format, bool inline, params object[] objs)
        {
            for (var i = 0; i < objs.Length; i++)
            {
                if (objs[i] is decimal decimalParameter)
                {
                    objs[i] = decimalParameter.ToString("0.###", CultureInfo.InvariantCulture);
                }
            }

            if (_initialized)
            {
                if (inline)
                {
                    _logStreamWriter.Write(format, objs);
                    Console.Write(format, objs);
                }
                else
                {
                    _logStreamWriter.WriteLine(format, objs);
                    Console.WriteLine(format, objs);
                }
            }
        }
    }
}
