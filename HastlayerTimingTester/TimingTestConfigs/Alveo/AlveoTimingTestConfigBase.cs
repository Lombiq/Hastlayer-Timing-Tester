namespace HastlayerTimingTester.TimingTestConfigs.Alveo;

internal class AlveoTimingTestConfigBase : XilinxUltraScalePlusTimingTestConfigBase
{
    public AlveoTimingTestConfigBase() =>
        // A process will use up to about 5,3 GB of RAM so more won't fit on a 32 GB RAM VM.
        NumberOfStaProcesses = 5;
}
