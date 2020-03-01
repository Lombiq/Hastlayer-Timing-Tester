using HastlayerTimingTester.Drivers;

namespace HastlayerTimingTester.TimingTestConfigs
{
    internal abstract class XilinxTimingTestConfigBase : TimingTestConfig
    {
        protected XilinxTimingTestConfigBase()
        {
            Driver = new XilinxDriver(this, @"D:\Xilinx\Vivado\2019.2\bin\vivado.bat");
        }
    }
}
