namespace HastlayerTimingTester.TimingTestConfigs.Zynq;

internal class ZedBoardTimingTestConfig : ZynqTimingTestConfigBase
{
    public ZedBoardTimingTestConfig()
    {
        Name = "ZedBoard";
        Part = "xc7z020clg484-1";
        FrequencyHz = 100e6m;
    }
}
