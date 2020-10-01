using System.Globalization;
using System.Text.RegularExpressions;

namespace HastlayerTimingTester.Parsers
{
    /// <summary>For passing the data output by Quartus into IntelParser.</summary>
    public class QuartusResult
    {
        public string SetupReport { get; set; }
        public string TimingSummary { get; set; }
    }

    /// <summary>
    /// Parser for Intel/Altera Quartus Prime Standard Edition 15.1 static timing analysis output.
    /// See the following Confluence page for more information:
    /// https://lombiq.atlassian.net/wiki/spaces/HAST/pages/186744859/Timing+on+Catapult.
    /// </summary>
    internal class IntelParser : TimingOutputParser
    {
        public IntelParser(decimal clockFrequency)
            : base(clockFrequency) { }

        /// <summary>
        /// Parses the STA output of Quartus.
        /// </summary>
        public void Parse(QuartusResult result)
        {
            // Data Path Delay
            var match = Regex.Match(result.SetupReport, @"; Data Delay(\s*);(\s*)([0-9\.\-]*)(\s*);");
            if (match.Success)
            {
                DataPathDelay = decimal.Parse(match.Groups[3].Value, CultureInfo.InvariantCulture);
                DataPathDelayAvailable = true;
            }

            // Let's see a sync design
            _extendedSyncParametersCount = 0;
            Requirement = 0;
            match = Regex.Match(result.SetupReport, @"; Setup Relationship(\s*); ([0-9\.\-]*)");
            if (match.Success)
            {
                Requirement = decimal.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture);
                _extendedSyncParametersCount++;
            }

            RequirementPlusDelays = 0;
            match = Regex.Match(result.SetupReport, @"; Data Required Time ; ([0-9\.\-]*)");
            if (match.Success)
            {
                RequirementPlusDelays = decimal.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
                _extendedSyncParametersCount++;
            }

            SourceClockDelay = 0;
            match = Regex.Match(result.SetupReport, @"; Data Arrival Time  ; ([0-9\.\-]*)");
            if (match.Success)
            {
                SourceClockDelay = decimal.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture) - DataPathDelay;
                _extendedSyncParametersCount++;
            }

            // Timing Summary
            var matches = Regex.Matches(result.TimingSummary, @"; clk   ; ([0-9\.\-]*) ; ([0-9\.\-]*)");
            if (matches.Count > 0)
            {
                WorstSetupSlack = decimal.Parse(matches[0].Groups[1].Value, CultureInfo.InvariantCulture);
                TotalSetupSlack = decimal.Parse(matches[0].Groups[2].Value, CultureInfo.InvariantCulture);
                WorstHoldSlack = decimal.Parse(matches[1].Groups[1].Value, CultureInfo.InvariantCulture);
                TotalHoldSlack = decimal.Parse(matches[1].Groups[2].Value, CultureInfo.InvariantCulture);
                WorstPulseWidthSlack = decimal.Parse(matches[2].Groups[1].Value, CultureInfo.InvariantCulture);
                TotalPulseWidthSlack = decimal.Parse(matches[2].Groups[2].Value, CultureInfo.InvariantCulture);
                TimingSummaryAvailable = true;
            }
        }
    }
}
