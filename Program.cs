using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace HastlayerTimingTester
{
    ///<summary>This is for passing the data output by Vivado into TimingOutputParser.</summary>
    struct VivadoResult
    {
        public string TimingReport;
        public string TimingSummary;
    }

    ///<summary>VHDL templates contain the hardware project to be compiled. They consist of a VHDL and an XDC
    ///(constraints file) template, both of which will be used by Vivado.</summary>
    abstract class VhdlTemplateBase
    {
        public string VhdlTemplate { get; protected set; }
        public string XdcTemplate { get; protected set; }
        abstract public string Name { get; }
    }

    ///<summary>VhdlOp provides data to fill a VHDL template with (see <see cref="VhdlString" /> and
    ///     <see cref="OutputDataTypeFunction" />).</summary>
    class VhdlOp
    {
        ///<summary>VhdlString contains the actual operator (like "+", "-", "mod", etc.) that will be subsituted
        ///     into the VHDL template.</summary>
        public string VhdlString;

        ///<summary>FriendlyName will be used in directory names, where you cannot use special characters. E.g. for "+"
        ///     a good FriendlyName is "add".</summary>
        public string FriendlyName;

        ///<summary>OutputDataTypeFunction can generate the output data type from the input data type and size. It
        ///     allows us to handle VHDL operators that have different input and output data types.</summary>
        public OutputDataTypeDelegate OutputDataTypeFunction;

        public VhdlOp(string vhdlString, string friendlyName, OutputDataTypeDelegate outputDataTypeFunction)
        {
            this.VhdlString = vhdlString;
            this.FriendlyName = friendlyName;
            this.OutputDataTypeFunction = outputDataTypeFunction;
        }

        public delegate string OutputDataTypeDelegate(
            int inputSize,
            TimingTestConfig.DataTypeFromSizeDelegate inputDataTypeFunction,
            bool getFriendlyName
        );

        ///<summary>SameOutputDataType is used if the output data type is the same as the input data type.</summary>
        public static string SameOutputDataType(
            int inputSize,
            TimingTestConfig.DataTypeFromSizeDelegate inputDataTypeFunction,
            bool getFriendlyName
        )
        {
            return inputDataTypeFunction(inputSize, getFriendlyName);
        }

        ///<summary>ComparisonWithBoolOutput is used for operators that strictly have boolean as their output data type
        ///     (like all comparison operators).</summary>
        public static string ComparisonWithBoolOutput(
            int inputSize,
            TimingTestConfig.DataTypeFromSizeDelegate inputDataTypeFunction,
            bool getFriendlyName
        )
        {
            return "boolean";
        }

        ///<summary>DoubleSizedOutput is used for operators whose output is the same type as their input, but with
        ///     double data size (e.g. multiplication).</summary>
        public static string DoubleSizedOutput(
            int inputSize,
            TimingTestConfig.DataTypeFromSizeDelegate inputDataTypeFunction,
            bool getFriendlyName
        )
        {
            return inputDataTypeFunction(inputSize * 2, getFriendlyName);
        }
    }

    ///<summary>This is the base class for configuration. For more information, check the
    ///     <see cref="TimingTestConfig" /> subclass.</summary>
    abstract class TimingTestConfigBase
    {
        ///<summary>This is used for <see cref="DataTypes" />.</summary>
        public delegate string DataTypeFromSizeDelegate(int size, bool getFriendlyName);
        public string Name;
        public List<VhdlOp> Operators;
        public List<VhdlTemplateBase> VhdlTemplates;
        public List<int> InputSizes;
        public string Part;
        public List<DataTypeFromSizeDelegate> DataTypes;
        public string VivadoPath;
        public bool DebugMode;
        public float Frequency;
        public bool VivadoBatchMode;
        public bool ImplementDesign;
    }


    class TimingTester
    {
        private TimingOutputParser _parser;
        private TimingTestConfigBase _test;

        ///<summary>This template is filled with data during the test, and then opened and ran by Vivado.
        ///It synthesizes the project, generates reports and a schematic diagram.</summary>
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

        ///<summary>This is like: @"TestResults\2016-09-15__10-52-19__default"</summary>
        string CurrentTestOutputBaseDirectory;

        ///<summary>This is like: @"TestResults\2016-09-15__10-52-19__default\gt_unsigned32_to_boolean_comb"</summary>
        string CurrentTestOutputDirectory;

        ///<summary>This function gets things ready before the test, then runs the test.
        ///     It creates the necessary directory structure, cleans up VivadoFiles and generates
        ///     a Tcl script for Vivado.</summary>
        public void InitializeTest(TimingTestConfigBase test)
        {
            _test = test;
            _parser = new TimingOutputParser(test.Frequency);
            //Clean the VivadoFiles directory (delete it recursively and mkdir):
            if (Directory.Exists("VivadoFiles")) Directory.Delete("VivadoFiles", true);
            Directory.CreateDirectory("VivadoFiles");
            File.WriteAllText(
                "VivadoFiles\\Generate.tcl",
                TclTemplate
                    .Replace("%PART%", _test.Part)
                    .Replace("%IMPLEMENT%", (Convert.ToInt32(_test.ImplementDesign)).ToString())
            );
            if (!Directory.Exists("TestResults")) Directory.CreateDirectory("TestResults");
            var timeNow = DateTime.Now;
            var currentTestDirectoryName = timeNow.ToString("yyyy-MM-dd__HH-mm-ss") + "__" + _test.Name;
            CurrentTestOutputBaseDirectory = "TestResults\\" + currentTestDirectoryName;
            if (Directory.Exists(CurrentTestOutputBaseDirectory))
            {
                Logger.Log("Fatal error: the test directory already exists ({0}), which is very unlikely" +
                    "because we used the date and time to generate the directory name.",
                    CurrentTestOutputBaseDirectory);
                return;
            }
            Directory.CreateDirectory(CurrentTestOutputBaseDirectory);
            Logger.Init(CurrentTestOutputBaseDirectory + "\\Log.txt", CurrentTestOutputBaseDirectory + "\\Results.tsv");
            Logger.WriteResult("Op\tInType\tOutType\tTemplate\tDesignStat\tDPD\tTWD\r\n");
            if (_test.VivadoBatchMode) Logger.Log("Vivado cannot generate Schematic.pdf for designs in batch mode.");
            Logger.Log("Starting analysis at: {0}", timeNow.ToString("yyyy-MM-dd HH:mm:ss"));
            RunTest();
            Logger.Log("Analysis finished at: {0}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        }

        ///<summary>It runs Vivado.</summary>
        string RunVivado(string vivadoPath, string tclFile, bool batchMode = false)
        {
            var cp = new Process();
            cp.StartInfo.FileName = vivadoPath;
            cp.StartInfo.Arguments = ((batchMode) ? " -mode batch" : "") + " -source " + tclFile;
            cp.StartInfo.WorkingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
                "\\VivadoFiles";
            //Logger.Log("WorkingDirectory = " + cp.StartInfo.WorkingDirectory);
            cp.StartInfo.UseShellExecute = !batchMode;
            cp.StartInfo.CreateNoWindow = false;
            cp.StartInfo.RedirectStandardOutput = false;
            cp.Start();
            cp.WaitForExit();
            //return cp.StandardOutput.ReadToEnd();
            return "";
        }

        ///<summary>It copies the given file from VivadoFiles to the output directory of the current test.</summary>
        void CopyFileToOutputDir(string inputPath)
        {
            File.Copy(inputPath, CurrentTestOutputDirectory + "\\" + Path.GetFileName(inputPath));
        }

        ///<summary>It runs tests for all combinations of operators, input data types,
        ///     data sizes and VHDL templates.</summary>
        void RunTest()
        {
            foreach (var op in _test.Operators)
            {
                foreach (var inputSize in _test.InputSizes)
                {
                    foreach (var inputDataTypeFunction in _test.DataTypes)
                    {
                        foreach (var vhdlTemplate in _test.VhdlTemplates)
                        {
                            try
                            {
                                Logger.Log("========================== starting test ==========================");
                                var inputDataType = inputDataTypeFunction(inputSize, false);
                                var outputDataType = op.OutputDataTypeFunction(
                                    inputSize,
                                    inputDataTypeFunction,
                                    false);

                                var uutPath = "VivadoFiles\\UUT.vhd";
                                var xdcPath = "VivadoFiles\\Constraints.xdc";
                                var synthTimingReportOutputPath = "VivadoFiles\\SynthTimingReport.txt";
                                var synthTimingSummaryOutputPath = "VivadoFiles\\SynthTimingSummary.txt";
                                var implTimingReportOutputPath = "VivadoFiles\\ImplTimingReport.txt";
                                var implTimingSummaryOutputPath = "VivadoFiles\\ImplTimingSummary.txt";
                                var schematicOutputPath = "VivadoFiles\\Schematic.pdf";

                                //To see if Vivado succeeded with the implementation, the existence of the text file at
                                //[implTimingReportOutputPath] is checked later.
                                //For that reason, we need to make sure this file does not exist at the beginning.
                                if (File.Exists(implTimingReportOutputPath)) File.Delete(implTimingReportOutputPath);
                                if (File.Exists(implTimingSummaryOutputPath)) File.Delete(implTimingSummaryOutputPath);
                                if (File.Exists(synthTimingReportOutputPath)) File.Delete(synthTimingReportOutputPath);
                                if (File.Exists(synthTimingSummaryOutputPath)) File.Delete(synthTimingSummaryOutputPath);

                                Logger.Log("Now generating: {0}({1}), {2}, {3} to {4}", op.FriendlyName, op.VhdlString,
                                    inputSize, inputDataType, outputDataType);
                                var testFriendlyName = string.Format("{0}_{1}_to_{2}_{3}",
                                    op.FriendlyName,
                                    inputDataTypeFunction(inputSize, true),
                                    op.OutputDataTypeFunction(inputSize, inputDataTypeFunction, true),
                                    vhdlTemplate.Name);
                                //friendly name should contain something from each "foreach" iterator
                                CurrentTestOutputDirectory = CurrentTestOutputBaseDirectory + "\\" + testFriendlyName;
                                Directory.CreateDirectory(CurrentTestOutputDirectory);
                                Logger.Log("\tDir name: {0}", testFriendlyName);

                                var vhdl = vhdlTemplate.VhdlTemplate
                                    .Replace("%INTYPE%", inputDataType)
                                    .Replace("%OUTTYPE%", outputDataType)
                                    .Replace("%OPERATOR%", op.VhdlString);
                                File.WriteAllText(uutPath, vhdl);
                                CopyFileToOutputDir(uutPath);
                                File.WriteAllText(
                                    xdcPath,
                                    vhdlTemplate.XdcTemplate.Replace("%CLKPERIOD%",
                                        ((1.0 / _test.Frequency) * 1e9F).ToString(CultureInfo.InvariantCulture))
                                );
                                CopyFileToOutputDir(xdcPath);

                                Logger.LogInline("Running Vivado... ");
                                RunVivado(_test.VivadoPath, "Generate.tcl", _test.VivadoBatchMode);
                                Logger.Log("Done.");
                                CopyFileToOutputDir(synthTimingReportOutputPath);
                                CopyFileToOutputDir(synthTimingSummaryOutputPath);
                                var ImplementationSuccessful = true;
                                if (File.Exists(implTimingReportOutputPath))
                                    CopyFileToOutputDir(implTimingReportOutputPath);
                                else ImplementationSuccessful = false;
                                if (File.Exists(implTimingSummaryOutputPath))
                                    CopyFileToOutputDir(implTimingSummaryOutputPath);
                                if (!_test.VivadoBatchMode) CopyFileToOutputDir(schematicOutputPath);

                                var synthVivadoResult = new VivadoResult();
                                synthVivadoResult.TimingReport = File.ReadAllText(synthTimingReportOutputPath);
                                synthVivadoResult.TimingSummary = File.ReadAllText(synthTimingSummaryOutputPath);
                                _parser.Parse(synthVivadoResult);
                                Logger.Log("Synthesis:\r\n----------");
                                _parser.PrintParsedTimingReport("S");
                                _parser.PrintParsedTimingSummary();

                                if (_test.ImplementDesign)
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

                                Logger.WriteResult("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\r\n",
                                    op.VhdlString,
                                    inputDataTypeFunction(inputSize, true),
                                    op.OutputDataTypeFunction(inputSize, inputDataTypeFunction, true),
                                    vhdlTemplate.Name,
                                    ((_test.ImplementDesign && ImplementationSuccessful) ? "impl" : "synth"),
                                    _parser.DataPathDelay,
                                    _parser.TimingWindowDiffFromRequirement
                                );
                                //return;
                            }
                            catch (Exception exception)
                            {
                                if (_test.DebugMode) throw;
                                else Logger.Log("Exception happened during test: {0}", exception.Message);
                            }
                        }
                    }
                }
            }

            Logger.Log("Finished, exiting.");
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var timingTester = new TimingTester();
            var test = new TimingTestConfig();
            timingTester.InitializeTest(test);
        }
    }

    ///<summary>Logger writes a formatted string to both a log file
    ///     (Log.txt in CurrentTestOutputBaseDirectory) and the
    ///     console. It also handles writing to the results file
    ///     (Results.tsv in CurrentTestOutputBaseDirectory).</summary>
    static class Logger
    {
        private static StreamWriter _logStreamWriter;
        private static StreamWriter _resultsStreamWriter;
        private static bool _initialized;

        ///<summary>This function initializes the Logger, to open the file given in LogFilePath.
        ///(Logger already works before initialization, but it only writes to the console.)</summary>
        static public void Init(string LogFilePath, string ResultsFilePath)
        {
            _logStreamWriter = new StreamWriter(File.Create(LogFilePath));
            _logStreamWriter.AutoFlush = true;
            _resultsStreamWriter = new StreamWriter(File.Create(ResultsFilePath));
            _resultsStreamWriter.AutoFlush = true;
            _initialized = true;
        }

        ///<summary>WriteResult writes a formatted string to the results file (if already initialized).</summary>
        static public void WriteResult(string Format, params object[] Objs)
        {
            if (_initialized) _resultsStreamWriter.Write(Format, Objs);
        }
        ///<summary>Log writes a formatted string to both a log file (if already initialized) and the console, ending
        ///     with a line break.</summary>
        static public void Log(string Format, params object[] Objs)
        {
            LogInternal(Format, false, Objs);
        }

        ///<summary>LogInline writes a formatted string to both a log file (if already initialized) and the console.
        ///     It does not end with a line break.</summary>
        static public void LogInline(string Format, params object[] Objs)
        {
            LogInternal(Format, true, Objs);
        }

        ///<summary>LogInternal implements the functionality described for <see cref="Logger.Log"/> and
        ///     <see cref="Logger.LogInline"/>.</summary>
        ///     <param name="Inline">It ends the line with a line break based on the Inline parameter.</param>
        static private void LogInternal(string Format, bool Inline, params object[] Objs)
        {
            for (var i = 0; i < Objs.Length; i++)
            {
                if (Objs[i].GetType() == typeof(float))
                {
                    Objs[i] = ((float)Objs[i]).ToString(CultureInfo.InvariantCulture);
                }
            }
            if (_initialized)
            {
                if (Inline)
                {
                    _logStreamWriter.Write(Format, Objs);
                    Console.Write(Format, Objs);
                }
                else
                {
                    _logStreamWriter.WriteLine(Format, Objs);
                    Console.WriteLine(Format, Objs);
                }
            }
        }

    }
}
