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
    class ResultWriter
    {
        private StreamWriter _resultsStreamWriter;
        private bool _initialized;
        /// <summary>
        /// This function initializes the Logger, to open the file given in LogFilePath.
        /// (Logger already works before initialization, but it only writes to the console.)
        /// </summary>
        public ResultWriter(string ResultsFilePath)
        {
            _resultsStreamWriter = new StreamWriter(File.Create(ResultsFilePath));
            _resultsStreamWriter.AutoFlush = true;
            _initialized = true;
        }
        /// <summary>WriteResult writes a formatted string to the results file (if already initialized).</summary>
        public void WriteResult(string Format, params object[] Objs)
        {
            if (_initialized) _resultsStreamWriter.Write(Format, Objs);
        }
    }
}
