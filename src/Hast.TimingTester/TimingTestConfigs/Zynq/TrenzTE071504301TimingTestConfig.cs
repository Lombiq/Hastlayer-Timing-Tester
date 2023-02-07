namespace HastlayerTimingTester.TimingTestConfigs.Zynq;

/// <summary>
/// Configuration for the Trenz Electronic TE0715-04-30-1C ("SoC Module with Xilinx Zynq XC7Z030-1SBG485C, 1 GByte DDR3L
/// SDRAM, 4 x 5 cm", see: <see
/// href="https://shop.trenz-electronic.de/en/TE0715-04-30-1C-SoC-Module-with-Xilinx-Zynq-XC7Z030-1SBG485C-1-GByte-DDR3L-SDRAM-4-x-5-cm"/>).
/// </summary>
internal class TrenzTE071504301TimingTestConfig : ZynqTimingTestConfigBase
{
    public TrenzTE071504301TimingTestConfig()
    {
        Name = "TE0715-04-30-1C";
        Part = "xc7z030sbg485-1";
        FrequencyHz = 100e6m;
    }
}
