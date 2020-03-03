using HastlayerTimingTester.Drivers;

namespace HastlayerTimingTester.TimingTestConfigs
{
    /// <summary>
    /// Common configuration for the largest Xilinx FPGAs.
    /// </summary>
    internal class XilinxUltraScalePlusTimingTestConfigBase : XilinxTimingTestConfigBase
    {
        public XilinxUltraScalePlusTimingTestConfigBase()
        {
            Driver = new XilinxDriver(this, @"C:\Xilinx\Vivado\2019.2\bin\vivado.bat");
            // A process will use more than 5GB of RAM so more than five won't fit even on a 32GB RAM VM.
            NumberOfSTAProcesses = 5;
        }
    }
}
