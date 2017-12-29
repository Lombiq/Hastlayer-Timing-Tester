using System;
using System.Globalization;
using System.IO;

namespace HastlayerTimingTester
{

    /// <summary>
    /// Logger writes a formatted string to both a log file
    /// (Log.txt in CurrentTestOutputBaseDirectory) and the
    /// console. It also handles writing to the results file
    /// (Results.tsv in CurrentTestOutputBaseDirectory).
    /// </summary>
    static class Logger
    {
        private static StreamWriter _logStreamWriter;
        private static bool _initialized;


        /// <summary>
        /// This function initializes the Logger, to open the file given in LogFilePath.
        /// (Logger already works before initialization, but it only writes to the console.)
        /// </summary>
        public static void Init(string logFilePath, bool createFile = true)
        {
            _logStreamWriter = new StreamWriter(File.Open(logFilePath,
                (createFile) ? FileMode.Create : FileMode.Append));
            _logStreamWriter.AutoFlush = true;
            _initialized = true;
        }

        /// <summary>
        /// Log writes a formatted string to both a log file (if already initialized) and the console, ending
        /// with a line break.
        /// </summary>
        public static void Log(string format, params object[] objs)
        {
            LogInternal(format, false, objs);
        }

        /// <summary>
        /// LogInline writes a formatted string to both a log file (if already initialized) and the console.
        /// It does not end with a line break.
        /// </summary>
        public static void LogInline(string format, params object[] objs)
        {
            LogInternal(format, true, objs);
        }

        /// <summary>
        /// It writes a header to the log and screen for a given processing stage of Hastlayer Timing Tester, 
        /// which can be specified as an input parameter (stage).
        /// </summary>
        public static void LogStageHeader(string stage)
        {
            Logger.Log("\r\n=== HastlayerTimingTester {0} stage ===", stage);
            Logger.Log("Started at {0}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        }


        /// <summary>
        /// LogInternal implements the functionality described for <see cref="Logger.Log"/> and
        /// <see cref="Logger.LogInline"/>.
        /// </summary>
        /// <param name="inline">It ends the line with a line break based on the Inline parameter.</param>
        private static void LogInternal(string format, bool inline, params object[] objs)
        {
            for (var i = 0; i < objs.Length; i++)
            {
                if (objs[i].GetType() == typeof(decimal))
                {
                    objs[i] = ((decimal)objs[i]).ToString("0.###", CultureInfo.InvariantCulture);
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
