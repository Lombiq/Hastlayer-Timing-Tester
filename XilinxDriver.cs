using System;

using System.Diagnostics;
using System.Globalization;
using System.IO;

namespace HastlayerTimingTester
{
    public class XilinxDriver : FpgaVendorDriver
    {
        private string _vivadoPath;
        public override bool CanStaAfterSynthesize { get { return true; } }
        public override bool CanStaAfterImplementation { get { return true; } }

        public XilinxDriver(TimingTestConfigBase t, string vivadoPath) : base(t)
        {
            _vivadoPath = vivadoPath;
        }

        /// <summary>
        /// This template is filled with data during the test, and then opened and ran by Vivado.
        /// It synthesizes the project, generates reports and a schematic diagram.
        /// </summary>
        const string TclTemplate = @"
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

        const string XdcTemplate = "create_clock -period %CLKPERIOD% -name clk [get_ports {clk}]";

        public override void InitPrepare(StreamWriter batchWriter)
        {
            base.InitPrepare(batchWriter);
            batchWriter.FormattedWriteLine("echo \"Vivado cannot generate Schematic.pdf for designs in batch mode.\"");
            File.WriteAllText(
                BaseDir + "\\Generate.tcl",
                TclTemplate
                    .Replace("%PART%", testConfig.Part)
                    .Replace("%IMPLEMENT%", (Convert.ToInt32(testConfig.ImplementDesign)).ToString())
            );
        }

        public override void Prepare(string outputDirectoryName, string vhdl, VhdlTemplateBase vhdlTemplate)
        {
            string uutPath = TimingTester.CurrentTestBaseDirectory + "\\" + outputDirectoryName + "\\UUT.vhd";
            string xdcPath = TimingTester.CurrentTestBaseDirectory + "\\" + outputDirectoryName + "\\Constraints.xdc";
            File.WriteAllText(uutPath, vhdl);
            File.WriteAllText(
                xdcPath,
                (vhdlTemplate.HasTimingConstraints) ?
                    XdcTemplate.Replace("%CLKPERIOD%",
                    ((1.0m / testConfig.Frequency) * 1e9m).ToString(CultureInfo.InvariantCulture)) : ""
            );

            batchWriter.FormattedWriteLine("cd {0}", outputDirectoryName);
            batchWriter.FormattedWriteLine("{0} -mode batch -source ../Generate.tcl", _vivadoPath);
            batchWriter.FormattedWriteLine("cd ..");

        }

        public override TimingOutputParser Analyze(string outputDirectoryName, StaPhase phase)
        {
            var parser = new XilinxParser(testConfig.Frequency);
            var synthTimingReportOutputPath = TimingTester.CurrentTestBaseDirectory + "\\" + outputDirectoryName + "\\SynthTimingReport.txt";
            var synthTimingSummaryOutputPath = TimingTester.CurrentTestBaseDirectory + "\\" + outputDirectoryName + "\\SynthTimingSummary.txt";
            var implTimingReportOutputPath = TimingTester.CurrentTestBaseDirectory + "\\" + outputDirectoryName + "\\ImplTimingReport.txt";
            var implTimingSummaryOutputPath = TimingTester.CurrentTestBaseDirectory + "\\" + outputDirectoryName + "\\ImplTimingSummary.txt";

            if (phase == StaPhase.Implementation && !testConfig.ImplementDesign)
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
            var result = new VivadoResult(); 
            result.TimingReport = File.ReadAllText((phase == StaPhase.Implementation) ? implTimingReportOutputPath : synthTimingReportOutputPath);
            result.TimingSummary = File.ReadAllText((phase == StaPhase.Implementation) ? implTimingSummaryOutputPath : synthTimingSummaryOutputPath);
            parser.Parse(result);
            return parser;
        }
    }
}
