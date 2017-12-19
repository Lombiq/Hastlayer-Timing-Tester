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
        static public void Init(string LogFilePath, bool CreateFile = true)
        {
            _logStreamWriter = new StreamWriter(File.Open(LogFilePath,
                (CreateFile) ? FileMode.Create : FileMode.Append));
            _logStreamWriter.AutoFlush = true;
            _initialized = true;
        }
        /// <summary>
        /// Log writes a formatted string to both a log file (if already initialized) and the console, ending
        /// with a line break.
        /// </summary>
        static public void Log(string Format, params object[] Objs)
        {
            LogInternal(Format, false, Objs);
        }
        /// <summary>
        /// LogInline writes a formatted string to both a log file (if already initialized) and the console.
        /// It does not end with a line break.
        /// </summary>
        static public void LogInline(string Format, params object[] Objs)
        {
            LogInternal(Format, true, Objs);
        }
        /// <summary>
        /// LogInternal implements the functionality described for <see cref="Logger.Log"/> and
        /// <see cref="Logger.LogInline"/>.
        /// </summary>
        /// <param name="Inline">It ends the line with a line break based on the Inline parameter.</param>
        static private void LogInternal(string Format, bool Inline, params object[] Objs)
        {
            for (var i = 0; i < Objs.Length; i++)
            {
                if (Objs[i].GetType() == typeof(decimal))
                {
                    Objs[i] = ((decimal)Objs[i]).ToString("0.###", CultureInfo.InvariantCulture);
                }
            }
            if (_initialized)
            {
                if (Inline)
                {
                    _logStreamWriter.Write(Format, Objs);
                    Console.Write(Format, Objs);
                }
                else
                {
                    _logStreamWriter.WriteLine(Format, Objs);
                    Console.WriteLine(Format, Objs);
                }
            }
        }

        public static void LogStageHeader(string stage)
        {
            Logger.Log("\r\n=== HastlayerTimingTester {0} stage ===", stage);
            Logger.Log("Started at {0}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        }
    }

}
