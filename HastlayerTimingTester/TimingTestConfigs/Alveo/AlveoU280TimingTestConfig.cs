namespace HastlayerTimingTester.TimingTestConfigs.Alveo
{
    internal class AlveoU280TimingTestConfig : AlveoTimingTestConfigBase
    {
        public AlveoU280TimingTestConfig()
        {
            Name = "AlveoU280";
            Part = "xcu280-fsvh2892-2L-e";
            FrequencyHz = 300e6m;
        }
    }
}
