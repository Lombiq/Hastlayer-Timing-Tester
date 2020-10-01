using HastlayerTimingTester.Parsers;
using HastlayerTimingTester.Vhdl;
using System.IO;
using System.Linq;

namespace HastlayerTimingTester.Drivers
{
    /// <summary>
    /// The base class of drivers for FPGA vendor tools (compilation, STA).
    /// </summary>
    internal abstract class FpgaVendorDriver
    {
        protected TimingTestConfig _testConfig;
        protected StreamWriter _batchWriter;

        /// <summary>The current test root directory.</summary>
        public string CurrentRootDirectoryPath;

        /// <summary>Gets a value indicating whether the tool can run STA after synthesis.</summary>
        public abstract bool CanStaAfterSynthesize { get; }

        /// <summary>Gets a value indicating whether the tool can run STA after implementation.</summary>
        public abstract bool CanStaAfterImplementation { get; }


        protected FpgaVendorDriver(TimingTestConfig testConfig) => _testConfig = testConfig;


        /// <summary>Prepare stage, ran for each test. Usually generates the batch file Run.bat.</summary>
        public abstract void Prepare(string outputDirectoryName, string vhdl, VhdlTemplateBase vhdlTemplate);

        /// <summary>Analyze stage, ran for each test.</summary>
        public abstract TimingOutputParser Analyze(string outputDirectoryName, StaPhase phase);

        /// <summary>Initialization of Prepare stage, generates scripts common for all tests.</summary>
        public virtual void InitPrepare(StreamWriter batchWriter) => _batchWriter = batchWriter;


        protected string CombineWithCurrentRootPath(params string[] subPaths) =>
            Path.Combine(new[] { CurrentRootDirectoryPath }.Union(subPaths).ToArray());
    }
}
