using System.IO;

namespace HastlayerTimingTester.Drivers
{
    internal static class RetryLogicStreamWriterExtensions
    {
        public static void BeginRetryWrapper(this StreamWriter batchWriter, string timingReportFilePath) =>
            batchWriter.WriteLine($"if not exist {timingReportFilePath} (");

        public static void EndRetryWrapper(this StreamWriter batchWriter) =>
            batchWriter.WriteLine(")");
    }
}
