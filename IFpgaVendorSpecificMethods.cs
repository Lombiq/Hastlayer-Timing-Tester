using System;
using System.IO;

namespace HastlayerTimingTester
{
    public abstract class FpgaVendorDriver
    {

        protected TimingTestConfigBase testConfig;
        protected StreamWriter batchWriter;
        public string BaseDir;
        public FpgaVendorDriver(TimingTestConfigBase testConfig)
        {
            this.testConfig = testConfig;
        }
        public abstract void Prepare(string outputDirectoryName, VhdlOp op, int inputSize, string inputDataType, string outputDataType,
            VhdlTemplateBase vhdlTemplate);
        public virtual void InitPrepare()
        {
            batchWriter = new StreamWriter(File.Open(BaseDir + "\\Run.bat", FileMode.Create));
        }
        //abstract public void InitPrepare();
        //void BatchFileCreate(string path);
        //void BatchFileCommitTest();
        //void BatchFileFinalize();
    }

    public static class StreamWriterExtension
    {
        public static void FormattedWriteLine(this StreamWriter writer, string format, params Object[] args)
        {
            writer.WriteLine(String.Format(format, args));
        }
    }
}

