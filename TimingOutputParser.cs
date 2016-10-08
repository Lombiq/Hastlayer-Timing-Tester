using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace HastlayerTimingTester
{

    class TimingOutputParser
    {
        ///<summary>It parses the timing report and timing summary output of Vivado. It makes some calculations based on these.
        ///It can also print the most important values.
        ///Look at the documentation (Docs/Introduction.md and Docs/Usage.md) for the meaning of the properties of this class.</summary>

        public float ClockFrequency;
        public TimingOutputParser(float clockFrequency) { ClockFrequency = clockFrequency; }
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
        public float RequirementPlusDelays { get; private set; }
        public float Requirement { get; private set; }
        public float SourceClockDelay { get; private set; }
        private int ExtendedSyncParametersCount;
        private bool ExtendedSyncParametersAvailable  { get { return ExtendedSyncParametersCount == 3; } }
        public float TimingWindowAvailable { get { return RequirementPlusDelays - SourceClockDelay; } }
        public float TimingWindowDiffFromRequirement { get { return TimingWindowAvailable - Requirement; } }
        public float MaxClockFrequency { get { return 1.0F/((DataPathDelay-TimingWindowDiffFromRequirement)*1.0e-9F); } }
        public float NanosecondToClockPeriod(float ns) { return (ns * 1.0e-9F)/(1.0F / ClockFrequency); }
        public float InMHz(float fHz) { return fHz/1e6F; } //Hz to MHz

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
            Requirement = 0;
            myMatch = Regex.Match(result.TimingReport, @"(\s*)Requirement:(\s*)([0-9\.]*)ns");
            if(myMatch.Success)
            {
                Requirement = float.Parse(myMatch.Groups[3].Value, CultureInfo.InvariantCulture);
                ExtendedSyncParametersCount++;
            }

            RequirementPlusDelays = 0;
            myMatch = Regex.Match(result.TimingReport, @"\n(\s*)required time(\s*)([0-9\.]*)(\s*)");
            if(myMatch.Success)
            {
                RequirementPlusDelays = float.Parse(myMatch.Groups[3].Value, CultureInfo.InvariantCulture);
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
                        if(timingSummaryLineParts[1]!="NA")
                        {
                            WorstNegativeSlack = float.Parse(timingSummaryLineParts[1], CultureInfo.InvariantCulture);
                            TotalNegativeSlack = float.Parse(timingSummaryLineParts[2], CultureInfo.InvariantCulture);
                            WorstHoldSlack = float.Parse(timingSummaryLineParts[5], CultureInfo.InvariantCulture);
                            TotalHoldSlack = float.Parse(timingSummaryLineParts[6], CultureInfo.InvariantCulture);
                            WorstPulseWidthSlack = float.Parse(timingSummaryLineParts[9], CultureInfo.InvariantCulture);
                            TotalPulseWidthSlack = float.Parse(timingSummaryLineParts[10], CultureInfo.InvariantCulture);
                            TimingSummaryAvailable = true;
                        }
                    }
                    catch (FormatException) { } //pass, at least TimingSummaryAvailable will stay false
                    break;
                }
            }

        }
        public void PrintParsedTimingReport(string Marker = "")
        {
            Logger.Log("Timing Report:");
            if(DataPathDelayAvailable)
                Logger.Log("\t{3}>> Data path delay = {0} ns  ({1} cycle at {2} MHz clock)", DataPathDelay, NanosecondToClockPeriod(DataPathDelay), InMHz(ClockFrequency), Marker);
            if(ExtendedSyncParametersAvailable)
            {
                Logger.Log(
                    "\tSource clock delay = {0} ns\r\n" +
                    "\tRequirement for arrival = {1} ns\r\n" +
                    "\tRequirement plus delays = {2} ns\r\n" +
                    "\tTiming window available = {3} ns\r\n" +
                    "\t{8}>> Timing window diff from requirement = {4} ns  ({5} cycle at {6} MHz clock)\r\n" +
                    "\tMax clock frequency = {7} MHz ",
                    SourceClockDelay,
                    Requirement,
                    RequirementPlusDelays,
                    TimingWindowAvailable,
                    TimingWindowDiffFromRequirement, NanosecondToClockPeriod(TimingWindowDiffFromRequirement), InMHz(ClockFrequency),
                    InMHz(MaxClockFrequency),
                    Marker
                    );
            }
        }
        public void PrintParsedTimingSummary()
        {
            if(TimingSummaryAvailable)
            {
                Logger.Log(
                    "Timing Summary:\r\n" +
                    "\tDesign {0} meeting timing requirements\r\n" +
                    "\tWorst Negative Slack = {1} ns\r\n" +
                    "\tTotal Negative Slack = {2} ns\r\n" +
                    "\tWorst Hold Slack = {3} ns\r\n" +
                    "\tTotal Hold Slack = {4} ns\r\n" +
                    "\tWorst Pulse Width Slack = {5} ns\r\n" +
                    "\tTotal Pulse Width Slack = {6} ns\r\n" +
                    "\t(Any \"worst slack\" is okay if positive,\r\n\t\tany \"total slack\" is okay if zero.)\r\n",
                    (DesignMetTimingRequirements) ? "PASSED" : "FAILED",
                    WorstNegativeSlack, TotalNegativeSlack,
                    WorstHoldSlack, TotalHoldSlack,
                    WorstPulseWidthSlack, TotalPulseWidthSlack
                );
                if(TotalNegativeSlack>0) Logger.Log("WARNING: setup time violation!");
                if(TotalHoldSlack>0) Logger.Log("WARNING: hold time violation!");
                if(TotalPulseWidthSlack>0) Logger.Log("WARNING: minimum pulse width violation!");
            }
            else Logger.Log("Timing summary did not contain slack values (or could not be parsed).");
        }

    }

}
