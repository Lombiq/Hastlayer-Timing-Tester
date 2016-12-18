using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace HastlayerTimingTester
{

    ///<summary>It parses the timing report and timing summary output of Vivado. It makes some calculations based on
    ///     these. It can also print the most important values. Look at the documentation
    ///     (Docs/Introduction.md and Docs/Usage.md) for the meaning of the properties of this class.</summary>
    class TimingOutputParser
    {
        public decimal ClockFrequency;
        public TimingOutputParser(decimal clockFrequency) { ClockFrequency = clockFrequency; }
        public decimal DataPathDelay { get; private set; }
        public bool DataPathDelayAvailable { get; private set; }
        public bool TimingSummaryAvailable { get; private set; }
        public decimal WorstNegativeSlack { get; private set; }
        public decimal TotalNegativeSlack { get; private set; }
        public decimal WorstHoldSlack { get; private set; }
        public decimal TotalHoldSlack { get; private set; }
        public decimal WorstPulseWidthSlack { get; private set; }
        public decimal TotalPulseWidthSlack { get; private set; }
        public bool DesignMetTimingRequirements
        {
            get
            {
                return TimingSummaryAvailable && TotalNegativeSlack == 0 &&
                    TotalHoldSlack == 0 &&
                    TotalPulseWidthSlack == 0;
            }
        }
        public decimal RequirementPlusDelays { get; private set; }
        public decimal Requirement { get; private set; }
        public decimal SourceClockDelay { get; private set; }
        private int _extendedSyncParametersCount;
        private bool ExtendedSyncParametersAvailable { get { return _extendedSyncParametersCount == 3; } }
        public decimal TimingWindowAvailable { get { return RequirementPlusDelays - SourceClockDelay; } }
        public decimal TimingWindowDiffFromRequirement { get { return TimingWindowAvailable - Requirement; } }
        public decimal MaxClockFrequency
        {
            get { return 1.0m / ((DataPathDelay - TimingWindowDiffFromRequirement) * 1.0e-9m); }
        }
        public decimal NanosecondToClockPeriod(decimal ns) { return (ns * 1.0e-9m) / (1.0m / ClockFrequency); }
        public decimal InMHz(decimal fHz) { return fHz / 1e6m; } //Hz to MHz

        public void Parse(VivadoResult result)
        {
            //Data Path Delay
            var match = Regex.Match(result.TimingReport, @"(\s*)Data Path Delay:(\s*)([0-9\.]*)ns");
            if (match.Success)
            {
                DataPathDelay = decimal.Parse(match.Groups[3].Value, CultureInfo.InvariantCulture);
                DataPathDelayAvailable = true;
            }

            //Let's see a sync design
            _extendedSyncParametersCount = 0;
            Requirement = 0;
            match = Regex.Match(result.TimingReport, @"(\s*)Requirement:(\s*)([0-9\.]*)ns");
            if (match.Success)
            {
                Requirement = decimal.Parse(match.Groups[3].Value, CultureInfo.InvariantCulture);
                _extendedSyncParametersCount++;
            }

            RequirementPlusDelays = 0;
            match = Regex.Match(result.TimingReport, @"\n(\s*)required time(\s*)([0-9\.]*)(\s*)");
            if (match.Success)
            {
                RequirementPlusDelays = decimal.Parse(match.Groups[3].Value, CultureInfo.InvariantCulture);
                _extendedSyncParametersCount++;
            }

            SourceClockDelay = 0;
            match = Regex.Match(result.TimingReport, @"(\s*)Source Clock Delay(\s*)\(SCD\):(\s*)([0-9\.]*)ns");
            if (match.Success)
            {
                SourceClockDelay = decimal.Parse(match.Groups[4].Value, CultureInfo.InvariantCulture);
                _extendedSyncParametersCount++;
            }

            //Timing Summary
            var timingSummaryLines = Regex.Split(result.TimingSummary, "\r\n").ToList();
            for (var i = 0; i < timingSummaryLines.Count; i++)
            {
                if (
                    timingSummaryLines[i].StartsWith("| Design Timing Summary") &&
                    timingSummaryLines[i + 1].StartsWith("| ---------------------")
                )
                {
                    var totalTimingSummaryLine = timingSummaryLines[i + 6];
                    while (totalTimingSummaryLine.Contains("  "))
                        totalTimingSummaryLine = totalTimingSummaryLine.Replace("  ", " ");
                    var timingSummaryLineParts =
                        totalTimingSummaryLine.Replace("  ", " ").Split(" ".ToCharArray()).ToList();
                    try
                    {
                        if (timingSummaryLineParts[1] != "NA")
                        {
                            WorstNegativeSlack = decimal.Parse(timingSummaryLineParts[1], CultureInfo.InvariantCulture);
                            TotalNegativeSlack = decimal.Parse(timingSummaryLineParts[2], CultureInfo.InvariantCulture);
                            WorstHoldSlack = decimal.Parse(timingSummaryLineParts[5], CultureInfo.InvariantCulture);
                            TotalHoldSlack = decimal.Parse(timingSummaryLineParts[6], CultureInfo.InvariantCulture);
                            WorstPulseWidthSlack = decimal.Parse(timingSummaryLineParts[9], CultureInfo.InvariantCulture);
                            TotalPulseWidthSlack = decimal.Parse(timingSummaryLineParts[10], CultureInfo.InvariantCulture);
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
            if (DataPathDelayAvailable)
                Logger.Log(
                    "\t{3}>> Data path delay = {0} ns  ({1} cycle at {2} MHz clock)",
                    DataPathDelay,
                    NanosecondToClockPeriod(DataPathDelay),
                    InMHz(ClockFrequency),
                    Marker
                );
            if (ExtendedSyncParametersAvailable)
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
                    TimingWindowDiffFromRequirement,
                    NanosecondToClockPeriod(TimingWindowDiffFromRequirement),
                    InMHz(ClockFrequency),
                    InMHz(MaxClockFrequency),
                    Marker
                    );
            }
        }
        public void PrintParsedTimingSummary()
        {
            if (TimingSummaryAvailable)
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
                if (TotalNegativeSlack > 0) Logger.Log("WARNING: setup time violation!");
                if (TotalHoldSlack > 0) Logger.Log("WARNING: hold time violation!");
                if (TotalPulseWidthSlack > 0) Logger.Log("WARNING: minimum pulse width violation!");
            }
            else Logger.Log("Timing summary did not contain slack values (or could not be parsed).");
        }

    }

}
