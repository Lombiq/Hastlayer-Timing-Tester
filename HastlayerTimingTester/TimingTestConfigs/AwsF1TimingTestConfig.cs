namespace HastlayerTimingTester.TimingTestConfigs
{
    /// <summary>
    /// Configuration for the FPGA in the AWS F1 instances (VU9P chip).
    /// </summary>
    /// <remarks>
    /// <para>
    /// There's not much info on the actual hardware. The part name is apparent from here: <see
    /// href="https://github.com/aws/aws-fpga/search?q=part_name&amp;unscoped_q=part_name"/>. The main clock is
    /// mentioned here: <see href="https://forums.aws.amazon.com/thread.jspa?threadID=257471"/>.
    /// </para>
    /// <para>
    /// Note that a license for the part being used is not included in a Vitis installation by default. But if you the
    /// open Xilinx Vivado License Manager and just go to Obtain License and opt to start a 30-day trial you'll be able
    /// to run all the tests.
    /// </para>
    /// </remarks>
    internal class AwsF1TimingTestConfig : XilinxUltraScalePlusTimingTestConfigBase
    {
        public AwsF1TimingTestConfig()
        {
            Name = "AWSF1";
            Part = "xcvu9p-flgb2104-2-i";
            FrequencyHz = 250e6m;
            // A process will usually use up to about 4,5 GB of RAM (with one-two processes bursting to 5 GB) so more
            // won't fit on a 32 GB RAM VM.
            NumberOfStaProcesses = 6;
        }
    }
}
