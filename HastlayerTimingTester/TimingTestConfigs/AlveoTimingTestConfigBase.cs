namespace HastlayerTimingTester.TimingTestConfigs
{
    internal class AlveoTimingTestConfigBase : XilinxUltraScalePlusTimingTestConfigBase
    {
        public AlveoTimingTestConfigBase()
        {
            // A process will use up to about 5,3 GB of RAM so more won't fit on a 32 GB RAM VM.
            NumberOfSTAProcesses = 5;
        }
    }
}
