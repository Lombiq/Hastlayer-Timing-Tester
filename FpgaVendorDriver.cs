using System;
using System.Diagnostics;
using System.IO;

namespace HastlayerTimingTester
{
    public abstract class FpgaVendorDriver
    {

        protected TimingTestConfigBase _testConfig;
        protected StreamWriter _batchWriter;
        public string BaseDir;
        public abstract bool CanStaAfterSynthesize { get; }
        public abstract bool CanStaAfterImplementation { get; }

        public FpgaVendorDriver(TimingTestConfigBase testConfig)
        {
            _testConfig = testConfig;
        }

        public abstract void Prepare(string outputDirectoryName, string vhdl, VhdlTemplateBase vhdlTemplate);
        public abstract TimingOutputParser Analyze(string outputDirectoryName, StaPhase phase);

        public virtual void InitPrepare(StreamWriter batchWriter)
        {
            _batchWriter = batchWriter;
        }
    }
    public static class StreamWriterExtension
    {
        public static void FormattedWriteLine(this StreamWriter writer, string format, params Object[] args)
        {
            writer.WriteLine(String.Format(format, args));
        }
    }
}

