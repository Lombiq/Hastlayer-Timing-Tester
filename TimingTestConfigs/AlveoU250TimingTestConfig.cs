﻿namespace HastlayerTimingTester.TimingTestConfigs
{
    internal class AlveoU250TimingTestConfig : XilinxTimingTestConfigBase
    {
        public AlveoU250TimingTestConfig()
        {
            Name = "AlveoU250";
            Part = "xcu250-figd2104-2L-e";
            Frequency = 300e6m;
        }
    }
}