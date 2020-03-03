namespace HastlayerTimingTester.TimingTestConfigs
{
    /// <summary>
    /// Configuration for the FPGA in the AWS F1 instances.
    /// </summary>
    /// <remarks>
    /// There's not much info on the actual hardware. The part name is apparent from here: 
    /// https://github.com/aws/aws-fpga/search?q=part_name&unscoped_q=part_name The main clock is mentioned here:
    /// https://forums.aws.amazon.com/thread.jspa?threadID=257471
    /// </remarks>
    internal class AwsF1TimingTestConfig : XilinxUltraScalePlusTimingTestConfigBase
    {
        public AwsF1TimingTestConfig()
        {
            Name = "AWSF1";
            Part = "xcvu9p-flgb2104-2-i";
            Frequency = 250e6m;
        }
    }
}
