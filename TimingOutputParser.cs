using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace HastlayerTimingTester
{

    class TimingOutputParser
    {
        public float DataPathDelay { get; private set; }
        public bool DataPathDelayAvailable { get; private set; }
        public bool TimingSummaryAvailable { get; private set; }
        public float WorstNegativeSlack { get; private set; }
        public float TotalNegativeSlack { get; private set; }
        public float WorstHoldSlack { get; private set; }
        public float TotalHoldSlack { get; private set; }
        public float WorstPulseWidthSlack { get; private set; }
        public float TotalPulseWidthSlack { get; private set; }
        public bool DesignMetTimingRequirements { get { return TimingSummaryAvailable && TotalNegativeSlack == 0 && TotalHoldSlack == 0 && TotalPulseWidthSlack == 0; } }
        public float RequiredTimeWithDelays { get; private set; }
        public float RequiredTime { get; private set; }
        public float SourceClockDelay { get; private set; }
        private int ExtendedSyncParametersCount;
        private bool ExtendedSyncParametersAvailable  { get { return ExtendedSyncParametersCount == 3; }  }

        public float Time { get; private set; }



        public void Parse(VivadoResult result)
        {
            //Data Path Delay
            Match myMatch = Regex.Match(result.TimingReport, @"(\s*)Data Path Delay:(\s*)([0-9\.]*)ns");
            if(myMatch.Success)
            {
                DataPathDelay = float.Parse(myMatch.Groups[3].Value, CultureInfo.InvariantCulture);
                DataPathDelayAvailable = true;
            }

            //Let's see a sync design
            ExtendedSyncParametersCount = 0;
            RequiredTime = 0;
            myMatch = Regex.Match(result.TimingReport, @"(\s*)Requirement:(\s*)([0-9\.]*)ns");
            if(myMatch.Success)
            {
                RequiredTime = float.Parse(myMatch.Groups[3].Value, CultureInfo.InvariantCulture);
                ExtendedSyncParametersCount++;
            }

            RequiredTimeWithDelays = 0;
            myMatch = Regex.Match(result.TimingReport, @"\n(\s*)required time(\s*)([0-9\.]*)(\s*)");
            if(myMatch.Success)
            {
                RequiredTime = float.Parse(myMatch.Groups[3].Value, CultureInfo.InvariantCulture);
                ExtendedSyncParametersCount++;
            }

            SourceClockDelay = 0;
            myMatch = Regex.Match(result.TimingReport, @"(\s*)Source Clock Delay(\s*)\(SCD\):(\s*)([0-9\.]*)ns");
            if(myMatch.Success)
            {
                SourceClockDelay = float.Parse(myMatch.Groups[4].Value, CultureInfo.InvariantCulture);
                ExtendedSyncParametersCount++;
            }

            //Timing Summary
            List<string> timingSummaryLines = Regex.Split(result.TimingSummary, "\r\n").ToList();
            for(int i=0; i<timingSummaryLines.Count; i++)
            {
                if(timingSummaryLines[i].StartsWith("| Design Timing Summary") && timingSummaryLines[i+1].StartsWith("| ---------------------"))
                {
                    string totalTimingSummaryLine = timingSummaryLines[i+6];
                    while(totalTimingSummaryLine.Contains("  ")) totalTimingSummaryLine = totalTimingSummaryLine.Replace("  ", " ");
                    List<string> timingSummaryLineParts = totalTimingSummaryLine.Replace("  ", " ").Split(" ".ToCharArray()).ToList();
                    try
                    {
                        WorstNegativeSlack = float.Parse(timingSummaryLineParts[1], CultureInfo.InvariantCulture);
                        TotalNegativeSlack = float.Parse(timingSummaryLineParts[2], CultureInfo.InvariantCulture);
                        WorstHoldSlack = float.Parse(timingSummaryLineParts[5], CultureInfo.InvariantCulture);
                        TotalHoldSlack = float.Parse(timingSummaryLineParts[6], CultureInfo.InvariantCulture);
                        WorstPulseWidthSlack = float.Parse(timingSummaryLineParts[9], CultureInfo.InvariantCulture);
                        TotalPulseWidthSlack = float.Parse(timingSummaryLineParts[10], CultureInfo.InvariantCulture);
                        TimingSummaryAvailable = true;
                    }
                    catch (FormatException) { } //pass, at least TimingSummaryAvailable will stay false
                    break;
                }
            }

        }
        public void PrintParsedTimingReport()
        {
            if(DataPathDelayAvailable) Logger.Log("Data path delay = {0} ns;  Max clock frequency = {1} MHz", DataPathDelay, Math.Floor((1 / (DataPathDelay * 1e-9)) / 1000) / 1000);
        }
        public void PrintParsedTimingSummary()
        {
            if(TimingSummaryAvailable) Logger.Log(
                "Timing Summary:\r\n" +
                "\tDesign {0} meeting timing requirements\r\n" +
                "\tWorst Negative Slack = {1} ns\r\n" +
                "\tTotal Negative Slack = {2} ns\r\n" +
                "\tWorst Hold Slack = {3} ns\r\n" +
                "\tTotal Hold Slack = {4} ns\r\n" +
                "\tWorst Pulse Width Slack = {5} ns\r\n" +
                "\tTotal Pulse Width Slack = {6} ns\r\n" +
                "\t(Worst slack is okay if positive, total slack is okay if zero.)\r\n",
                (DesignMetTimingRequirements) ? "PASSED" : "FAILED",
                WorstNegativeSlack, TotalNegativeSlack,
                WorstHoldSlack, TotalHoldSlack,
                WorstPulseWidthSlack, TotalPulseWidthSlack
            );
            else Logger.Log("Timing summary did not contain slack values (or could not be parsed).\r\n\tThis is okay for an async UUT without a clock.");
        }

    }

}
