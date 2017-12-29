﻿using CommandLine;
using CommandLine.Text;

namespace HastlayerTimingTester
{
    class ProgramOptions
    {
        [Option('p', "prepare", Required = false, DefaultValue = false,
            HelpText = "Generate VHDL files and batch file for next step.")]
        public bool Prepare { get; set; }

        [Option('e', "exec-sta", DefaultValue = false,
          HelpText = "Run vendor-provided compiler toolchain and static timing analysis software. " +
            "Note that you can do this manually by running the batch file." +
            "This even allows you to run this step on another computer, although you have to give the paths " +
            "correctly in the configuration.")]
        public bool ExecSta { get; set; }

        [Option('a', "analyze", DefaultValue = false,
          HelpText = "Parse report files generated by vendor-provided compiler toolchain and " +
            "static timing analysis software, calculate timing parameters for Hastlayer.")]
        public bool Analyze { get; set; }

        [Option('x', "all", DefaultValue = false,
          HelpText = "Run all of the above.")]
        public bool All { get; set; }

        [Option('r', "all-remote-sta", DefaultValue = false,
          HelpText = "Run all of the above, except wait for the user to manually execute the -exec-sta step.")]
        public bool AllRemoteSta { get; set; }

        [ParserState]
        public IParserState LastParserState { get; set; }

        [HelpOption]
        public string GetUsage() =>
             HelpText.AutoBuild(this, (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
    }

    class Program
    {
        // Consume them
        static void Main(string[] args)
        {
            var options = new ProgramOptions();
            // This is a hack but I couldn't find an easy way to display the help if no parameters are present:
            if (args.Length == 0) args = new string[] { "--help" };
            if (CommandLine.Parser.Default.ParseArguments(args, options))
            {
                var timingTester = new TimingTester();
                var test = new TimingTestConfig();
                timingTester.DoTest(test, options);
            }
        }
    }
}
