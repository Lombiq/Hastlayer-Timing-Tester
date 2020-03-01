using HastlayerTimingTester.Drivers;

namespace HastlayerTimingTester.TimingTestConfigs
{
    internal class CatapultTimingTestConfig : TimingTestConfig
    {
        public CatapultTimingTestConfig()
        {
            Name = "Catapult";
            Driver = new IntelDriver(this, @"C:\altera\15.1\quartus\bin64");
            Frequency = 150e6m;
        }
    }
}
