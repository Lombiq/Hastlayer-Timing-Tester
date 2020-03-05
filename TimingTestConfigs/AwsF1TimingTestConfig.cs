namespace HastlayerTimingTester.TimingTestConfigs
{
    /// <summary>
    /// Configuration for the FPGA in the AWS F1 instances.
    /// </summary>
    /// <remarks>
    /// There's not much info on the actual hardware. The part name is apparent from here: 
    /// https://github.com/aws/aws-fpga/search?q=part_name&unscoped_q=part_name The main clock is mentioned here:
    /// https://forums.aws.amazon.com/thread.jspa?threadID=257471
    /// 
    /// Note that a license for the part being used is not included in a Vitis installation by default. But if you the 
    /// open Xilinx Vivado License Manager and just go to Obtain License and opt to start a 30-day trial you'll be able
    /// to run all the tests.
    /// </remarks>
    internal class AwsF1TimingTestConfig : XilinxUltraScalePlusTimingTestConfigBase
    {
        public AwsF1TimingTestConfig()
        {
            Name = "AWSF1";
            Part = "xcvu9p-flgb2104-2-i";
            Frequency = 250e6m;
            // A process will use up to about 4,5 GB of RAM so more won't fit on a 32 GB RAM VM.
            NumberOfSTAProcesses = 6;
        }
    }
}
