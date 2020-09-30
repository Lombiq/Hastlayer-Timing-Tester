using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace HastlayerTimingTester.Parsers
{
    /// <summary>For passing the data output by Vivado into TimingOutputParser.</summary>
    public class VivadoResult
    {
        public string TimingReport { get; set; }
        public string TimingSummary { get; set; }
    }

    /// <summary>
    /// Parser for Vivado static timing analysis output.
    /// </summary>
    internal class XilinxParser : TimingOutputParser
    {
        public XilinxParser(decimal clockFrequency)
            : base(clockFrequency) { }


        /// <summary>
        /// Parses the STA output of Vivado.
        /// </summary>
        public void Parse(VivadoResult result)
        {
            // Data Path Delay
            var match = Regex.Match(result.TimingReport, @"(\s*)Data Path Delay:(\s*)([0-9\.]*)ns");
            if (match.Success)
            {
                DataPathDelay = decimal.Parse(match.Groups[3].Value, CultureInfo.InvariantCulture);
                DataPathDelayAvailable = true;
            }

            // Let's see a sync design
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

            // Timing Summary
            var timingSummaryLines = Regex.Split(result.TimingSummary, "{Environment.NewLine}").ToList();
            for (var i = 0; i < timingSummaryLines.Count; i++)
            {
                if (timingSummaryLines[i].StartsWith("| Design Timing Summary", StringComparison.InvariantCulture) &&
                    timingSummaryLines[i + 1].StartsWith("| ---------------------", StringComparison.InvariantCulture))
                {
                    var totalTimingSummaryLine = timingSummaryLines[i + 6];
                    while (totalTimingSummaryLine.Contains("  ", StringComparison.InvariantCulture))
                        totalTimingSummaryLine = totalTimingSummaryLine.Replace("  ", " ", StringComparison.InvariantCulture);
                    var timingSummaryLineParts =
                        totalTimingSummaryLine.Replace("  ", " ", StringComparison.InvariantCulture).Split(" ".ToCharArray()).ToList();
                    try
                    {
                        if (timingSummaryLineParts[1] != "NA")
                        {
                            WorstSetupSlack = decimal.Parse(timingSummaryLineParts[1], CultureInfo.InvariantCulture);
                            TotalSetupSlack = decimal.Parse(timingSummaryLineParts[2], CultureInfo.InvariantCulture);
                            WorstHoldSlack = decimal.Parse(timingSummaryLineParts[5], CultureInfo.InvariantCulture);
                            TotalHoldSlack = decimal.Parse(timingSummaryLineParts[6], CultureInfo.InvariantCulture);
                            WorstPulseWidthSlack = decimal.Parse(timingSummaryLineParts[9], CultureInfo.InvariantCulture);
                            TotalPulseWidthSlack = decimal.Parse(timingSummaryLineParts[10], CultureInfo.InvariantCulture);
                            TimingSummaryAvailable = true;
                        }
                    }
                    catch (FormatException)
                    {
                        // Pass, at least TimingSummaryAvailable will stay false.
                    }
                }
            }
        }
    }
}
