namespace HastlayerTimingTester.TimingTestConfigs.Zynq
{
    /// <summary>
    /// Configuration for the Trenz Electronic TE0715-04-30-1C ("SoC Module with Xilinx Zynq XC7Z030-1SBG485C, 1 GByte
    /// DDR3L SDRAM, 4 x 5 cm", see: <see
    /// href="https://shop.trenz-electronic.de/en/TE0715-04-30-1C-SoC-Module-with-Xilinx-Zynq-XC7Z030-1SBG485C-1-GByte-DDR3L-SDRAM-4-x-5-cm"/>).
    /// </summary>
    internal class TE0715TimingTestConfig : ZynqTimingTestConfigBase
    {
        public TE0715TimingTestConfig()
        {
            Name = "TE0715";
            Part = "xc7z030sbg485-1";
            FrequencyHz = 300e6m;
        }
    }
}
