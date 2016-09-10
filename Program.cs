using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace hastlayer_timing_tester
{
    struct vivadoResult
    {
        public string timingReport;
        public string timingSummary;
    }

    abstract class vhdlTemplateBase
    {
        protected string _template;
        public string template { get{ return _template; } }
        abstract public void processResults(vivadoResult result);
    }

    class vhdlOp
    {
        public delegate string outputDataType(int inputSize, TimingTestConfig.dataTypeFromSize inputDataTypeFunction, bool getFriendlyName);
        public static string sameOutputDataType(int inputSize, TimingTestConfig.dataTypeFromSize inputDataTypeFunction, bool getFriendlyName)
            { return inputDataTypeFunction(inputSize, getFriendlyName); }
        public static string comparisonWithBoolOutput(int inputSize, TimingTestConfig.dataTypeFromSize inputDataTypeFunction, bool getFriendlyName)
            { return "boolean"; }
        public static string doubleSizedOutput(int inputSize, TimingTestConfig.dataTypeFromSize inputDataTypeFunction, bool getFriendlyName)
            { return inputDataTypeFunction(inputSize * 2, getFriendlyName); }

        public string vhdlString;
        public string friendlyName;
        public outputDataType outputDataTypeFunction;
        public vhdlOp(string vhdlString, string friendlyName, outputDataType outputDataTypeFunction)
            { this.vhdlString = vhdlString; this.friendlyName = friendlyName; this.outputDataTypeFunction = outputDataTypeFunction; }
    }

    abstract class TimingTestConfigBase
    {
        public delegate string dataTypeFromSize(int size, bool getFriendlyName);
        public string name; //this will be used for the directory name
        public List<vhdlOp> operators;
        public List<vhdlTemplateBase> vhdlTemplates;
        public List<int> inputSizes;
        public string part;
        public List<dataTypeFromSize> dataTypes;
        public string vivadoPath;
        public bool debugMode;
        public abstract void Populate();
        public TimingTestConfigBase() { Populate(); }
    }

    class TimingTestConfig : TimingTestConfigBase
    {
        public override void Populate()
        {
            operators = new List<vhdlOp>
            {
                new vhdlOp(">",     "gt",   vhdlOp.comparisonWithBoolOutput),
                new vhdlOp("<",     "lt",   vhdlOp.comparisonWithBoolOutput),
                new vhdlOp(">=",    "ge",   vhdlOp.comparisonWithBoolOutput),
                new vhdlOp("<=",    "le",   vhdlOp.comparisonWithBoolOutput),
                new vhdlOp("=",     "eq",   vhdlOp.comparisonWithBoolOutput),
                new vhdlOp("/=",    "neq",  vhdlOp.comparisonWithBoolOutput),
                new vhdlOp("+",     "add",  vhdlOp.sameOutputDataType),
                new vhdlOp("-",     "sub",  vhdlOp.sameOutputDataType),
                new vhdlOp("/",     "div",  vhdlOp.sameOutputDataType),
                new vhdlOp("*",     "mul",  vhdlOp.doubleSizedOutput),
                new vhdlOp("mod",   "mod",  vhdlOp.sameOutputDataType),
            };
            inputSizes = new List<int> { 32 };
            dataTypes = new List<dataTypeFromSize> {
                (size, getFriendlyName) => { return (getFriendlyName) ? String.Format("unsigned{0}", size) : String.Format("unsigned({0} downto 0)", size-1); },
                (size, getFriendlyName) => { return (getFriendlyName) ? String.Format("signed{0}", size) : String.Format("signed({0} downto 0)", size-1); }
            };
            part = "xc7a100tcsg324-1";
            vhdlTemplates = new List<vhdlTemplateBase> { new vhdlTemplateSync(), new vhdlTemplateAsync() };
            vivadoPath = "C:\\Xilinx\\Vivado\\2016.2\\bin\\vivado.bat";
            name = "default";
            debugMode = true;
        }

        int vhdlOpSizeMul(int input_size) { return 2*input_size; } //multiplication needs 2 times wider output than input
    }

    class TimingTester
    {
        TimingTestConfigBase test;

        const string tclTemplate = @"
read_vhdl UUT.vhd
synth_design -part %PART% -top tf_sample
config_timing_analysis -disable_flight_delays true
report_timing -file Timing.txt
report_timing_summary -check_timing_verbose -file TimingSummary.txt
quit
#
#report_timing_summary -delay_type min_max -report_unconstrained -check_timing_verbose -max_paths 10 -input_pins -name timing_1
#opt_design
#place_design
#route_design";

        string currentTestOutputBaseDirectory;
        string currentTestOutputDirectory;

        public void initializeTest(TimingTestConfigBase _test)
        {
            test = _test;
            if (Directory.Exists("VivadoFiles")) Directory.Delete("VivadoFiles", true); //Clean the VivadoFiles directory (delete it recursively and mkdir)
            Directory.CreateDirectory("VivadoFiles");
            File.WriteAllText("VivadoFiles\\Generate.tcl", tclTemplate.Replace("%PART%", test.part));
            if (!Directory.Exists("TestResults")) Directory.CreateDirectory("TestResults");
            string currentTestDirectoryName = DateTime.Now.ToString("yyyy-MM-dd__hh-mm-ss")+"__"+test.name;
            currentTestOutputBaseDirectory = "TestResults\\"+currentTestDirectoryName;
            if(Directory.Exists(currentTestOutputBaseDirectory))
            { Console.WriteLine("Fatal error: the test directory already exists ({0}), which is very unlikely" +
                "because we used the date and time to generate the directory name.", currentTestOutputBaseDirectory); return; }
            Directory.CreateDirectory(currentTestOutputBaseDirectory);
            runTest();
        }

        string runVivado(string vivadoPath, string tclFile)
        {
            Process cp = new Process();
            cp.StartInfo.FileName = vivadoPath;
            cp.StartInfo.Arguments = " -source " + tclFile;
            cp.StartInfo.WorkingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\VivadoFiles";
            //Console.WriteLine("WorkingDirectory = " + cp.StartInfo.WorkingDirectory);
            cp.StartInfo.UseShellExecute = true;
            cp.StartInfo.CreateNoWindow = false;
            //cp.StartInfo.RedirectStandardOutput = true;
            cp.Start();
            cp.WaitForExit();
            //return cp.StandardOutput.ReadToEnd();
            return "";
        }

        void copyFileToOutputDir(string inputPath)
        {
            File.Copy(inputPath, currentTestOutputDirectory+"\\"+Path.GetFileName(inputPath));
        }

        void runTest()
        {
            foreach (vhdlTemplateBase myVhdlTemplate in test.vhdlTemplates)
                foreach (vhdlOp op in test.operators)
                    foreach (int inputSize in test.inputSizes)
                        foreach (TimingTestConfigBase.dataTypeFromSize inputDataTypeFunction in test.dataTypes)
                        {
                            try
                            {
                                Console.WriteLine("========================== starting test ==========================");
                                string inputDataType = inputDataTypeFunction(inputSize, false);
                                string outputDataType = op.outputDataTypeFunction(inputSize, inputDataTypeFunction, false);
                                string uutPath = "VivadoFiles\\UUT.vhd";
                                string timingReportOutputPath = "VivadoFiles\\Timing.txt";
                                string timingSummaryOutputPath = "VivadoFiles\\TimingSummary.txt";
                                Console.WriteLine("Now generating: {0}({1}), {2}, {3} to {4}", op.friendlyName, op.vhdlString, inputSize, inputDataType, outputDataType);
                                string testFriendlyName = String.Format("{0}_{1}_to_{2}", op.friendlyName, inputDataTypeFunction(inputSize, true), op.outputDataTypeFunction(inputSize, inputDataTypeFunction, true));
                                currentTestOutputDirectory = currentTestOutputBaseDirectory + "\\" + testFriendlyName;
                                Directory.CreateDirectory(currentTestOutputDirectory);
                                Console.WriteLine("\tDir name: {0}", testFriendlyName);
                                string vhdl = myVhdlTemplate.template
                                    .Replace("%INTYPE%", inputDataType)
                                    .Replace("%OUTTYPE%", outputDataType)
                                    .Replace("%OPERATOR%", op.vhdlString);
                                File.WriteAllText(uutPath, vhdl);
                                copyFileToOutputDir(uutPath);
                                Console.Write("Running Vivado... ");
                                runVivado(test.vivadoPath, "Generate.tcl");
                                Console.WriteLine("Done.");
                                copyFileToOutputDir(timingReportOutputPath);
                                copyFileToOutputDir(timingSummaryOutputPath);
                                vivadoResult myVivadoResult = new vivadoResult();
                                myVivadoResult.timingReport = File.ReadAllText(timingReportOutputPath);
                                myVivadoResult.timingSummary = File.ReadAllText(timingSummaryOutputPath);
                                myVhdlTemplate.processResults(myVivadoResult);
                                //return;
                            }
                            catch(Exception myException)
                            {
                                if (test.debugMode) throw;
                                else Console.WriteLine("Exception happened during test: {0}", myException.Message);
                            }
                        }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            TimingTester myTimingTester = new TimingTester();
            TimingTestConfigBase test = new TimingTestConfig();
            myTimingTester.initializeTest(test);
        }
    }
}
