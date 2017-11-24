using System;

namespace HastlayerTimingTester
{
    public abstract class FpgaVendorDriver
    {

        protected TimingTestConfigBase testConfig;
        public string BaseDir;
        public FpgaVendorDriver(TimingTestConfigBase testConfig)
        {
            this.testConfig = testConfig;
        }
        public abstract void Prepare(string outputDirectory, VhdlOp op, int inputSize, string inputDataType, string outputDataType,
            VhdlTemplateBase vhdlTemplate);
        public abstract void InitPrepare();
        //abstract public void InitPrepare();
        //void BatchFileCreate(string path);
        //void BatchFileCommitTest();
        //void BatchFileFinalize();
    }
}
