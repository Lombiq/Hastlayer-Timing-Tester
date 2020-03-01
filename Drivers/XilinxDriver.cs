using HastlayerTimingTester.Parsers;
using HastlayerTimingTester.Vhdl;
using System;
using System.Globalization;
using System.IO;

namespace HastlayerTimingTester.Drivers
{
    /// <summary>
    /// Driver for the Xilinx FPGA tools (Vivado).
    /// For example, it contains templates for files to be generated for these tools.
    /// </summary>
    public class XilinxDriver : FpgaVendorDriver
    {
        private readonly string _vivadoPath;

        /// <summary>
        /// Xilinx tools support STA both after synthesis and implementation. 
        /// </summary>
        public override bool CanStaAfterSynthesize => true;

        /// <summary>
        /// Xilinx tools support STA both after synthesis and implementation. 
        /// </summary>
        public override bool CanStaAfterImplementation => true;


        /// <param name="vivadoPath">The path for the Vivado executable.</param>
        public XilinxDriver(TimingTestConfigBase testConfig, string vivadoPath) : base(testConfig)
        {
            _vivadoPath = vivadoPath;
        }


        /// <summary>
        /// To be filled with data, and then later opened and ran by Vivado.
        /// It synthesizes and optionally implements the project, generates reports and a schematic diagram.
        /// </summary>
        private const string _tclTemplate = @"
set_param general.maxThreads %NUMTHREADS%
read_vhdl UUT.vhd
read_xdc Constraints.xdc
synth_design -part %PART% -top tf_sample
config_timing_analysis -disable_flight_delays true
report_timing -file SynthTimingReport.txt
report_timing_summary -check_timing_verbose -file SynthTimingSummary.txt
show_schematic [get_nets]
write_schematic -force -format pdf -orientation landscape Schematic.pdf
if {%IMPLEMENT% == 0} { quit }
opt_design
place_design
route_design
report_timing -file ImplTimingReport.txt
report_timing_summary -check_timing_verbose -file ImplTimingSummary.txt
quit
";
        /// <summary>
        /// Template for the constraints file.
        /// </summary>
        private const string _xdcTemplate = "create_clock -period %CLKPERIOD% -name clk [get_ports {clk}]";

        /// <summary>Initialization of Prepare stage, generates scripts common for all tests.</summary>
        public override void InitPrepare(StreamWriter batchWriter)
        {
            base.InitPrepare(batchWriter);
            if (_testConfig.VivadoBatchMode)
                batchWriter.WriteLine("echo \"Vivado cannot generate Schematic.pdf for designs in batch mode.\"");
            File.WriteAllText(
                Path.Combine(CurrentRootDirectoryPath, "Generate.tcl"),
                _tclTemplate
                    .Replace("%NUMTHREADS%", _testConfig.NumberOfThreadsPerProcess.ToString())
                    .Replace("%PART%", _testConfig.Part)
                    .Replace("%IMPLEMENT%", (Convert.ToInt32(_testConfig.ImplementDesign)).ToString())
            );
        }

        /// <summary>Prepare stage, ran for each test. Generates the batch file Run.bat.</summary>
        public override void Prepare(string outputDirectoryName, string vhdl, VhdlTemplateBase vhdlTemplate)
        {
            var uutPath = CombineWithCurrentRootPath(outputDirectoryName, "UUT.vhd");
            var xdcPath = CombineWithCurrentRootPath(outputDirectoryName, "Constraints.xdc");
            File.WriteAllText(uutPath, vhdl);
            File.WriteAllText(
                xdcPath,
                (vhdlTemplate.HasTimingConstraints) ?
                    _xdcTemplate.Replace("%CLKPERIOD%",
                    ((1.0m / _testConfig.Frequency) * 1e9m).ToString(CultureInfo.InvariantCulture)) : ""
            );

            _batchWriter.WriteLine("cd {0}", outputDirectoryName);
            _batchWriter.BeginRetryWrapper("ImplTimingSummary.txt");
            _batchWriter.WriteLine("cmd /c \"{0} {1} -source ../Generate.tcl\"",
                _vivadoPath, (_testConfig.VivadoBatchMode) ? "-mode batch" : "");
            _batchWriter.EndRetryWrapper();
            _batchWriter.WriteLine("cd ..");
        }

        /// <summary>Analyze stage, ran for each test.</summary>
        public override TimingOutputParser Analyze(string outputDirectoryName, StaPhase phase)
        {
            var parser = new XilinxParser(_testConfig.Frequency);
            var synthTimingReportOutputPath =
                CombineWithCurrentRootPath(outputDirectoryName, "SynthTimingReport.txt");
            var synthTimingSummaryOutputPath =
                CombineWithCurrentRootPath(outputDirectoryName, "SynthTimingSummary.txt");
            var implTimingReportOutputPath =
                CombineWithCurrentRootPath(outputDirectoryName, "ImplTimingReport.txt");
            var implTimingSummaryOutputPath =
                CombineWithCurrentRootPath(outputDirectoryName, "ImplTimingSummary.txt");

            if (phase == StaPhase.Implementation && !_testConfig.ImplementDesign)
                throw new Exception("Can't analyze for implementation if ImplementDesign is false in the config.");

            if (phase == StaPhase.Implementation)
            {
                var ImplementationSuccessful = File.Exists(implTimingReportOutputPath);
                if (!ImplementationSuccessful)
                {
                    Logger.Log("Implementation (or STA) failed!");
                    return null;
                }
            }
            var result = new VivadoResult
            {
                TimingReport = File.ReadAllText(
                    (phase == StaPhase.Implementation) ? implTimingReportOutputPath : synthTimingReportOutputPath),
                TimingSummary = File.ReadAllText(
                    (phase == StaPhase.Implementation) ? implTimingSummaryOutputPath : synthTimingSummaryOutputPath)
            };
            parser.Parse(result);

            return parser;
        }
    }
}
