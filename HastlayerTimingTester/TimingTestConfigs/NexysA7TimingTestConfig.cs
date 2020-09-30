using HastlayerTimingTester.Drivers;

namespace HastlayerTimingTester.TimingTestConfigs
{
    internal class NexysA7TimingTestConfig : XilinxTimingTestConfigBase
    {
        public NexysA7TimingTestConfig()
        {
            Name = "NexysA7";
            Driver = new XilinxDriver(this, @"C:\Xilinx\Vivado\2016.4\bin\vivado.bat");
            Part = "xc7a100tcsg324-1";
            Frequency = 100e6m;
        }
    }
}
