using HastlayerTimingTester.Drivers;

namespace HastlayerTimingTester.TimingTestConfigs
{
    /// <summary>
    /// Configuration for Microsoft's Project Catapult. Altera (Intel) Quartus Prime and TimeQuest, version 15.1 are
    /// needed and optionally also Python 2.7 (https://www.python.org/downloads/) for running Cleanup.py.
    /// </summary>
    internal class CatapultTimingTestConfig : TimingTestConfig
    {
        public CatapultTimingTestConfig()
        {
            Name = "Catapult";
            Driver = new IntelDriver(this, @"C:\altera\15.1\quartus\bin64");
            FrequencyHz = 150e6m;
        }
    }
}
