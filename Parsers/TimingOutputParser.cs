namespace HastlayerTimingTester.Parsers
{
    public enum StaPhase
    {
        Synthesis, Implementation
    }

    /// <summary>
    /// Parses the timing report and timing summary output of Vivado. It makes some calculations based on
    /// these. It can also print the most important values. Look at the documentation
    /// (Docs/Introduction.md and Docs/Usage.md) for the meaning of the properties of this class.
    /// </summary>
    public abstract class TimingOutputParser
    {
        public decimal ClockFrequency;
        public TimingOutputParser(decimal clockFrequency) { ClockFrequency = clockFrequency; }
        public decimal DataPathDelay { get; protected set; }
        public bool DataPathDelayAvailable { get; protected set; }
        public bool TimingSummaryAvailable { get; protected set; }
        public decimal WorstSetupSlack { get; protected set; }
        public decimal TotalSetupSlack { get; protected set; }
        public decimal WorstHoldSlack { get; protected set; }
        public decimal TotalHoldSlack { get; protected set; }
        public decimal WorstPulseWidthSlack { get; protected set; }
        public decimal TotalPulseWidthSlack { get; protected set; }
        public bool DesignMetTimingRequirements
            => TimingSummaryAvailable && TotalSetupSlack == 0 && TotalHoldSlack == 0 && TotalPulseWidthSlack == 0;
        public decimal RequirementPlusDelays { get; protected set; }
        public decimal Requirement { get; protected set; }
        public decimal SourceClockDelay { get; protected set; }
        protected int _extendedSyncParametersCount;
        private bool ExtendedSyncParametersAvailable => _extendedSyncParametersCount == 3;
        public decimal TimingWindowAvailable => RequirementPlusDelays - SourceClockDelay;
        public decimal TimingWindowDiffFromRequirement => TimingWindowAvailable - Requirement;
        public decimal MaxClockFrequency => 1.0m / ((DataPathDelay - TimingWindowDiffFromRequirement) * 1.0e-9m);
        public decimal NanosecondToClockPeriod(decimal ns) => (ns * 1.0e-9m) / (1.0m / ClockFrequency);
        /// <summary>Converts Hz to Mhz.</summary>
        public decimal InMHz(decimal fHz) => fHz / 1e6m;

        /// <summary>Prints the parsed timing report.</summary>
        /// <param name="marker">It is shown in the printed output, to differentiate between 
        /// synthesis ("S") and implementation ("I").</param>
        public void PrintParsedTimingReport(string marker = "")
        {
            Logger.Log("Timing Report:");
            if (DataPathDelayAvailable)
                Logger.Log(
                    "\t{3}>> Data path delay = {0} ns  ({1} cycle at {2} MHz clock)",
                    DataPathDelay,
                    NanosecondToClockPeriod(DataPathDelay),
                    InMHz(ClockFrequency),
                    marker
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
                    marker
                    );
            }
        }

        /// <summary>Prints the parsed timing summary.</summary>
        public void PrintParsedTimingSummary()
        {
            if (TimingSummaryAvailable)
            {
                Logger.Log(
                    "Timing Summary:\r\n" +
                    "\tDesign {0} meeting timing requirements\r\n" +
                    "\tWorst Setup Slack = {1} ns\r\n" +
                    "\tTotal Setup Slack = {2} ns\r\n" +
                    "\tWorst Hold Slack = {3} ns\r\n" +
                    "\tTotal Hold Slack = {4} ns\r\n" +
                    "\tWorst Pulse Width Slack = {5} ns\r\n" +
                    "\tTotal Pulse Width Slack = {6} ns\r\n" +
                    "\t(Any \"worst slack\" is okay if positive,\r\n\t\tany \"total slack\" is okay if zero.)",
                    (DesignMetTimingRequirements) ? "PASSED" : "FAILED",
                    WorstSetupSlack, TotalSetupSlack,
                    WorstHoldSlack, TotalHoldSlack,
                    WorstPulseWidthSlack, TotalPulseWidthSlack
                );
                if (TotalSetupSlack > 0) Logger.Log("WARNING: setup time violation!");
                if (TotalHoldSlack > 0) Logger.Log("WARNING: hold time violation!");
                if (TotalPulseWidthSlack > 0) Logger.Log("WARNING: minimum pulse width violation!");
            }
            else Logger.Log("Timing summary did not contain slack values (or could not be parsed).");
        }
    }
}
