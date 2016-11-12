using System;
using System.Globalization;
using System.IO;

namespace HastlayerTimingTester
{

    ///<summary>Logger writes a formatted string to both a log file
    ///     (Log.txt in CurrentTestOutputBaseDirectory) and the
    ///     console. It also handles writing to the results file
    ///     (Results.tsv in CurrentTestOutputBaseDirectory).</summary>
    static class Logger
    {
        private static StreamWriter _logStreamWriter;
        private static StreamWriter _resultsStreamWriter;
        private static bool _initialized;
        ///<summary>This function initializes the Logger, to open the file given in LogFilePath.
        ///(Logger already works before initialization, but it only writes to the console.)</summary>
        static public void Init(string LogFilePath, string ResultsFilePath)
        {
            _logStreamWriter = new StreamWriter(File.Create(LogFilePath));
            _logStreamWriter.AutoFlush = true;
            _resultsStreamWriter = new StreamWriter(File.Create(ResultsFilePath));
            _resultsStreamWriter.AutoFlush = true;
            _initialized = true;
        }
        ///<summary>WriteResult writes a formatted string to the results file (if already initialized).</summary>
        static public void WriteResult(string Format, params object[] Objs)
        {
            if (_initialized) _resultsStreamWriter.Write(Format, Objs);
        }
        ///<summary>Log writes a formatted string to both a log file (if already initialized) and the console, ending
        ///     with a line break.</summary>
        static public void Log(string Format, params object[] Objs)
        {
            LogInternal(Format, false, Objs);
        }
        ///<summary>LogInline writes a formatted string to both a log file (if already initialized) and the console.
        ///     It does not end with a line break.</summary>
        static public void LogInline(string Format, params object[] Objs)
        {
            LogInternal(Format, true, Objs);
        }
        ///<summary>LogInternal implements the functionality described for <see cref="Logger.Log"/> and
        ///     <see cref="Logger.LogInline"/>.</summary>
        ///     <param name="Inline">It ends the line with a line break based on the Inline parameter.</param>
        static private void LogInternal(string Format, bool Inline, params object[] Objs)
        {
            for (var i = 0; i < Objs.Length; i++)
            {
                if (Objs[i].GetType() == typeof(decimal))
                {
                    Objs[i] = ((decimal)Objs[i]).ToString(CultureInfo.InvariantCulture);
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
    }

}
