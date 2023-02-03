using HastlayerTimingTester.Drivers;

namespace HastlayerTimingTester.TimingTestConfigs;

/// <summary>
/// Common configuration for the largest Xilinx FPGAs.
/// </summary>
internal class XilinxUltraScalePlusTimingTestConfigBase : XilinxTimingTestConfigBase
{
    public XilinxUltraScalePlusTimingTestConfigBase() =>
        Driver = new XilinxDriver(this, @"C:\Xilinx\Vivado\2020.1\bin\vivado.bat");
}
