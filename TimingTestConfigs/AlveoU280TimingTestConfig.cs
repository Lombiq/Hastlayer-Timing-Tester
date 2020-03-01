namespace HastlayerTimingTester.TimingTestConfigs
{
    internal class AlveoU280TimingTestConfig : XilinxTimingTestConfigBase
    {
        public AlveoU280TimingTestConfig()
        {
            Name = "AlveoU280";
            Part = "xcu280-fsvh2892-2L-e";
            Frequency = 300e6m;
        }
    }
}
