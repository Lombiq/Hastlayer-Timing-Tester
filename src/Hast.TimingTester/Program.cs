using CommandLine;
using HastlayerTimingTester.TimingTestConfigs;

namespace HastlayerTimingTester;

/// <summary>
/// Parameters for the command-line argument parser (CommandLine).
/// </summary>
internal class ProgramParameters
{
    [Option('p', "prepare", Required = false, Default = false, HelpText = "Generate VHDL files and batch file for next step.")]
    public bool Prepare { get; set; }

    [Option(
        'e',
        "exec-sta",
        Default = false,
        HelpText = "Run vendor-provided compiler toolchain and static timing analysis software. " +
            "Note that you can do this manually by running the batch file. " +
            "This even allows you to run this step on another computer, although you have to give the paths " +
            "correctly in the configuration.")]
    public bool ExecSta { get; set; }

    [Option(
        'a',
        "analyze",
        Default = false,
        HelpText = "Parse report files generated by vendor-provided compiler toolchain and " +
            "static timing analysis software, calculate timing parameters for Hastlayer.")]
    public bool Analyze { get; set; }

    [Option('x', "all", Default = false, HelpText = "Run all of the above.")]
    public bool All { get; set; }

    [Option(
        'r',
        "all-remote-sta",
        Default = false,
        HelpText = "Run all of the above, except wait for the user to manually execute the -exec-sta step.")]
    public bool AllRemoteSta { get; set; }
}

public static class Program
{
    // CS0162 is disabled because it would be a violation with useInlineConfiguration = false. IDE0079 is disabled so it
    // doesn't cause a violation if useInlineConfiguration = true.
#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable CS0162 // Unreachable code detected
    public static void Main(string[] args)
    {
        const bool useInlineConfiguration = true;

        // The const is a configuration.
        // This is a hack but couldn't find an easy way to display the help if no parameters are present:
        if (!useInlineConfiguration && args.Length == 0) args = new[] { "--help" };

        Parser.Default.ParseArguments<ProgramParameters>(args).WithParsed(parameters =>
        {
            if (useInlineConfiguration)
            {
                // Uncomment the one you want to use if you don't want to supply parameters as command line
                // arguments.
                parameters.Prepare = true;
                ////parameters.ExecSta = true;
                ////parameters.Analyze = true;
            }

            new TimingTester(new NexysA7TimingTestConfig()).DoTests(parameters);
        });
    }
#pragma warning restore CS0162 // Unreachable code detected
#pragma warning restore IDE0079 // Remove unnecessary suppression
}
