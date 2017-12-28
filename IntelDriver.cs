using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HastlayerTimingTester
{
    public class IntelDriver : FpgaVendorDriver
    {
        private string _quartusPath;
        public override bool CanStaAfterSynthesize { get => false; }
        public override bool CanStaAfterImplementation { get => true; }

        public IntelDriver(TimingTestConfigBase testConfig, string quartusPath) : base(testConfig)
        {
            _quartusPath = quartusPath;
        }

        /// <summary>
        /// This template is filled with data during the test, and then opened and ran by Quartus.
        /// It synthesizes and optionally implements the project.
        /// </summary>
        private const string _quartusTclTemplate = @"
# Quartus Prime: Generate Tcl File for Project
# Load Quartus Prime Tcl Project package
package require ::quartus::project
package require ::quartus::flow

project_new -revision tf_sample tf_sample -overwrite

# Make assignments
set_global_assignment -name FAMILY ""Stratix V""
set_global_assignment -name DEVICE 5SGSMD5H2F35I3L
set_global_assignment -name ORIGINAL_QUARTUS_VERSION ""15.1.1 SP1.03""
set_global_assignment -name PROJECT_CREATION_TIME_DATE ""04:08:45  NOVEMBER 04, 2017""
set_global_assignment -name LAST_QUARTUS_VERSION ""15.1.1 SP1.03""
set_global_assignment -name PROJECT_OUTPUT_DIRECTORY output_files
set_global_assignment -name MIN_CORE_JUNCTION_TEMP ""-40""
set_global_assignment -name MAX_CORE_JUNCTION_TEMP 100
set_global_assignment -name ERROR_CHECK_FREQUENCY_DIVISOR 256
set_global_assignment -name EDA_SIMULATION_TOOL ""ModelSim-Altera (VHDL)""
set_global_assignment -name EDA_OUTPUT_DATA_FORMAT VHDL -section_id eda_simulation
set_global_assignment -name PARTITION_NETLIST_TYPE SOURCE -section_id Top
set_global_assignment -name PARTITION_FITTER_PRESERVATION_LEVEL PLACEMENT_AND_ROUTING -section_id Top
set_global_assignment -name PARTITION_COLOR 16764057 -section_id Top
set_global_assignment -name SDC_FILE Constraints.sdc
set_global_assignment -name VHDL_FILE UUT.vhd
set_global_assignment -name POWER_PRESET_COOLING_SOLUTION ""23 MM HEAT SINK WITH 200 LFPM AIRFLOW""
set_global_assignment -name POWER_BOARD_THERMAL_MODEL ""NONE (CONSERVATIVE)""
set_instance_assignment -name PARTITION_HIERARCHY root_partition -to | -section_id Top

# Commit assignments
export_assignments

execute_flow -compile
project_close
";

        private const string _timeQuestTclTemplate = @"
project_open -force ""tf_sample.qpf"" -revision tf_sample
create_timing_netlist -model slow
read_sdc
update_timing_netlist
report_timing -from_clock { clk } -to_clock { clk } -setup -npaths 1 -detail full_path -file ""SetupReport.txt"" -stdout
#report_min_pulse_width -nworst 10 -detail full_path -file ""MinimumPulseWidthReport.txt""
create_timing_summary -setup -multi_corner -file TimingSummary.txt
create_timing_summary -hold -multi_corner -append -file TimingSummary.txt
create_timing_summary -mpw -multi_corner -append -file TimingSummary.txt
";

        private const string _sdcTemplate = @"
# Time Information
set_time_format -unit ns -decimal_places 3

# Create Clock
create_clock -name {clk} -period %CLKPERIOD% -waveform { 0.0 %CLKHALFPERIOD% } [get_ports {clk}]

# Set Clock Uncertainty
set_clock_uncertainty -rise_from [get_clocks {clk}] -rise_to [get_clocks {clk}] -setup 0.070  
set_clock_uncertainty -rise_from [get_clocks {clk}] -rise_to [get_clocks {clk}] -hold 0.060  
set_clock_uncertainty -rise_from [get_clocks {clk}] -fall_to [get_clocks {clk}] -setup 0.070  
set_clock_uncertainty -rise_from [get_clocks {clk}] -fall_to [get_clocks {clk}] -hold 0.060  
set_clock_uncertainty -fall_from [get_clocks {clk}] -rise_to [get_clocks {clk}] -setup 0.070  
set_clock_uncertainty -fall_from [get_clocks {clk}] -rise_to [get_clocks {clk}] -hold 0.060  
set_clock_uncertainty -fall_from [get_clocks {clk}] -fall_to [get_clocks {clk}] -setup 0.070  
set_clock_uncertainty -fall_from [get_clocks {clk}] -fall_to [get_clocks {clk}] -hold 0.060  
";
        private const string _cleanupScriptTemplate = @"
import os, shutil
subdirs=filter(lambda x:os.path.isdir(x), os.listdir("".""))
for subdir in subdirs:
    items=os.listdir(subdir)
    items=filter(lambda x: not x in [""UUT.vhd"", ""Constraints.sdc"", ""SetupReport.txt"", ""TimingSummary.txt""], items)
    paths=map(lambda x: subdir+""\\""+x, items)
    for path in paths:
        print ""unlinking: ""+path
        if os.path.isdir(path):
            shutil.rmtree(path)
        else:
            os.unlink(path)
";

        private const string _cleanupScriptName = "Cleanup.py";

        public override void InitPrepare(StreamWriter batchWriter)
        {
            base.InitPrepare(batchWriter);
            File.WriteAllText(BaseDir + "\\Quartus.tcl", _quartusTclTemplate);
            File.WriteAllText(BaseDir + "\\TimeQuest.tcl", _timeQuestTclTemplate);
            File.WriteAllText(BaseDir + "\\" + _cleanupScriptName, _cleanupScriptTemplate);
        }

        public override void Prepare(string outputDirectoryName, string vhdl, VhdlTemplateBase vhdlTemplate)
        {
            var uutPath = TimingTester.CurrentTestBaseDirectory + "\\" + outputDirectoryName + "\\UUT.vhd";
            var sdcPath = TimingTester.CurrentTestBaseDirectory + "\\" + outputDirectoryName + "\\Constraints.sdc";
            File.WriteAllText(uutPath, vhdl);
            var sdcContent = _sdcTemplate
                .Replace("%CLKPERIOD%", ((1.0m / _testConfig.Frequency) * 1e9m).ToString(CultureInfo.InvariantCulture))
                .Replace("%CLKHALFPERIOD%", ((0.5m / _testConfig.Frequency) * 1e9m).ToString(CultureInfo.InvariantCulture));
            File.WriteAllText(sdcPath, (vhdlTemplate.HasTimingConstraints) ? sdcContent : "");

            _batchWriter.FormattedWriteLine("cd {0}", outputDirectoryName);
            _batchWriter.FormattedWriteLine("{0}\\quartus_sh.exe -t ../Quartus.tcl", _quartusPath);
            _batchWriter.FormattedWriteLine("{0}\\quartus_sta.exe -t ../TimeQuest.tcl", _quartusPath);
            _batchWriter.FormattedWriteLine("cd ..");

        }

        public override TimingOutputParser Analyze(string outputDirectoryName, StaPhase phase)
        {
            var parser = new IntelParser(_testConfig.Frequency);
            var setupReportOutputPath = TimingTester.CurrentTestBaseDirectory + "\\" + outputDirectoryName + "\\SetupReport.txt";
            var timingSummaryOutputPath = TimingTester.CurrentTestBaseDirectory + "\\" + outputDirectoryName + "\\TimingSummary.txt";

            if (phase != StaPhase.Implementation)
            {
                throw new Exception("IntelDriver can't run STA right after synthesis, " +
                    "although ImplementDesign is true in the config.");
            }

            var implementationSuccessful =
                File.Exists(setupReportOutputPath) && File.Exists(timingSummaryOutputPath);

            if (!implementationSuccessful)
            {
                Logger.Log("STA failed!");
                return null;
            }

            var result = new QuartusResult();
            result.SetupReport = File.ReadAllText(setupReportOutputPath);
            result.TimingSummary = File.ReadAllText(timingSummaryOutputPath);
            parser.Parse(result);
            return parser;
        }
    }
}
