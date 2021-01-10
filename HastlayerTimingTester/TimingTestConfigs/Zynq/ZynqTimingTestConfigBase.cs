using HastlayerTimingTester.Drivers;

namespace HastlayerTimingTester.TimingTestConfigs.Zynq
{
    internal class ZynqTimingTestConfigBase : XilinxTimingTestConfigBase
    {
        public ZynqTimingTestConfigBase()
        {
            Driver = new XilinxDriver(this, @"C:\Xilinx\Vivado\2020.2\bin\vivado.bat");
            // A process will use up to about ? GB of RAM so more won't fit on a 32 GB RAM VM.
            NumberOfStaProcesses = 6;
        }
    }
}
