using HastlayerTimingTester.Drivers;

namespace HastlayerTimingTester.TimingTestConfigs.Zynq;

internal class ZynqTimingTestConfigBase : XilinxTimingTestConfigBase
{
    public ZynqTimingTestConfigBase()
    {
        Driver = new XilinxDriver(this, @"C:\Xilinx\Vivado\2020.2\bin\vivado.bat");
        // A process will use up to about 2 GB of RAM but saturate 8 CPU cores so more won't fit on an 8-core VM despite
        // it having 32 GB RAM.
        NumberOfStaProcesses = 6;
    }
}
