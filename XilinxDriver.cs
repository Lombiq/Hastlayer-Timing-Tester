using System;

using System.Diagnostics;
using System.Globalization;
using System.IO;

namespace HastlayerTimingTester
{
    public class XilinxDriver : FpgaVendorDriver
    {
        public XilinxDriver(TimingTestConfigBase t) : base(t) { }

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

        //string synthTimingReportOutputPath { get { return BaseDir + "SynthTimingReport.txt"; }}
        //string synthTimingSummaryOutputPath { get { return BaseDir + "SynthTimingSummary.txt"; }}
        //string implTimingReportOutputPath { get { return BaseDir + "ImplTimingReport.txt"; }}
        //string implTimingSummaryOutputPath { get { return BaseDir + "ImplTimingSummary.txt"; }}
        //string schematicOutputPath { get { return BaseDir + "Schematic.pdf"; }}

        public override void InitPrepare() 
        {
            base.InitPrepare();
            File.WriteAllText(
                BaseDir + "\\Generate.tcl",
                TclTemplate
                    .Replace("%PART%", testConfig.Part)
                    .Replace("%IMPLEMENT%", (Convert.ToInt32(testConfig.ImplementDesign)).ToString())
            );
        }

        public override void Prepare(string outputDirectoryName, VhdlOp op, int inputSize, string inputDataType, string outputDataType,
            VhdlTemplateBase vhdlTemplate)
        {
            string uutPath = TimingTester.CurrentTestBaseDirectory + "\\" + outputDirectoryName + "\\UUT.vhd";
            string xdcPath = TimingTester.CurrentTestBaseDirectory + "\\" + outputDirectoryName + "\\Constraints.xdc";
            var vhdl = vhdlTemplate.VhdlTemplate
                .Replace("%INTYPE%", inputDataType)
                .Replace("%OUTTYPE%", outputDataType)
                .Replace("%OPERATOR%", op.VhdlString);
            File.WriteAllText(uutPath, vhdl);
            File.WriteAllText(
                xdcPath,
                vhdlTemplate.XdcTemplate.Replace("%CLKPERIOD%",
                    ((1.0m / testConfig.Frequency) * 1e9m).ToString(CultureInfo.InvariantCulture))
            );

            batchWriter.FormattedWriteLine("cd {0}", outputDirectoryName);
            batchWriter.FormattedWriteLine("{0} -mode batch -source ../Generate.tcl", testConfig.VivadoPath);
            batchWriter.FormattedWriteLine("cd ..");

        }
        /*
        public override void StaPrepare()
        {

            //TODO put this into batch file if (_testConfig.VivadoBatchMode) Logger.Log("Vivado cannot generate Schematic.pdf for designs in batch mode.");

        }
        public void ExecSta()
        {
            Logger.LogInline("Running Vivado... ");
            RunVivado(_testConfig.VivadoPath, "Generate.tcl", _testConfig.VivadoBatchMode);
            Logger.Log("Done.");
            var ImplementationSuccessful = true;
        }

        public void Analyze()
        {
            // To see if Vivado succeeded with the implementation, the existence of the text file at
            // [implTimingReportOutputPath] is needed to be checked later. TODO

            if (_testConfig.ImplementDesign)
            {
                if (!ImplementationSuccessful) Logger.Log("Implementation (or STA) failed!");
                else
                {
                    var implVivadoResult = new VivadoResult();
                    implVivadoResult.TimingReport = File.ReadAllText(implTimingReportOutputPath);
                    implVivadoResult.TimingSummary = File.ReadAllText(implTimingSummaryOutputPath);
                    _parser.Parse(implVivadoResult);
                    Logger.Log("Implementation:\r\n---------------");
                    _parser.PrintParsedTimingReport("I");
                    _parser.PrintParsedTimingSummary();
                }
            }

            bool useImplementationResults = _testConfig.ImplementDesign && ImplementationSuccessful &&
                synthDataPathDelay + synthTimingWindowDiffFromRequirement <
                _parser.DataPathDelay + _parser.TimingWindowDiffFromRequirement;

            var synthVivadoResult = new VivadoResult();
            synthVivadoResult.TimingReport = File.ReadAllText(synthTimingReportOutputPath);
            synthVivadoResult.TimingSummary = File.ReadAllText(synthTimingSummaryOutputPath);
            _parser.Parse(synthVivadoResult);
            Logger.Log("Synthesis:\r\n----------");
            _parser.PrintParsedTimingReport("S");
            _parser.PrintParsedTimingSummary();
            var synthDataPathDelay = _parser.DataPathDelay;
            var synthTimingWindowDiffFromRequirement = _parser.TimingWindowDiffFromRequirement;

            Logger.WriteResult("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\r\n",
                op.VhdlString,
                inputDataTypeFunction(inputSize, true),
                op.OutputDataTypeFunction(inputSize, inputDataTypeFunction, true),
                vhdlTemplate.Name,
                (useImplementationResults) ? "impl" : "synth",
                (useImplementationResults) ? _parser.DataPathDelay : synthDataPathDelay,
                (useImplementationResults) ? _parser.TimingWindowDiffFromRequirement : synthTimingWindowDiffFromRequirement
            );

        }

        /// <summary>Runs Vivado in a separate process.</summary>
        // TODO: will we need this at all?
        void RunVivado(string vivadoPath, string tclFile, bool batchMode = false)
        {
            var cp = new Process();
            cp.StartInfo.FileName = vivadoPath;
            cp.StartInfo.Arguments = ((batchMode) ? " -mode batch" : "") + " -source " + tclFile;
            cp.StartInfo.WorkingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
                "\\VivadoFiles";
            // We could log the working directory of the new process:
            // Logger.Log("WorkingDirectory = " + cp.StartInfo.WorkingDirectory);
            cp.StartInfo.UseShellExecute = !batchMode;
            cp.StartInfo.CreateNoWindow = false;
            cp.StartInfo.RedirectStandardOutput = false;
            cp.Start();
            cp.WaitForExit();
            return;
        }
        */
    }
}
