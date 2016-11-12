using System;

namespace HastlayerTimingTester
{

    class Program
    {
        static void Main(string[] args)
        {
            var timingTester = new TimingTester();
            var test = new TimingTestConfig();
            timingTester.InitializeTest(test);
        }
    }

}
