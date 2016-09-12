using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace HastlayerTimingTester
{
    struct VivadoResult
    {
        public string TimingReport;
        public string TimingSummary;
    }

    abstract class VhdlTemplateBase
    {
        protected string _Template;
        public string Template { get{ return _Template; } }
        protected string _Xdc;
        public string Xdc { get{ return _Xdc; } }
        abstract public string Name { get; }
    }

    class VhdlOp
    {
        public delegate string OutputDataTypeDelegate(int inputSize, TimingTestConfig.DataTypeFromSizeDelegate inputDataTypeFunction, bool getFriendlyName);
        public static string SameOutputDataType(int inputSize, TimingTestConfig.DataTypeFromSizeDelegate inputDataTypeFunction, bool getFriendlyName)
            { return inputDataTypeFunction(inputSize, getFriendlyName); }
        public static string ComparisonWithBoolOutput(int inputSize, TimingTestConfig.DataTypeFromSizeDelegate inputDataTypeFunction, bool getFriendlyName)
            { return "boolean"; }
        public static string DoubleSizedOutput(int inputSize, TimingTestConfig.DataTypeFromSizeDelegate inputDataTypeFunction, bool getFriendlyName)
            { return inputDataTypeFunction(inputSize * 2, getFriendlyName); }

        public string VhdlString;
        public string FriendlyName;
        public OutputDataTypeDelegate OutputDataTypeFunction;
        public VhdlOp(string vhdlString, string friendlyName, OutputDataTypeDelegate outputDataTypeFunction)
            { this.VhdlString = vhdlString; this.FriendlyName = friendlyName; this.OutputDataTypeFunction = outputDataTypeFunction; }
    }

    abstract class TimingTestConfigBase
    {
        public delegate string DataTypeFromSizeDelegate(int size, bool getFriendlyName);
        public string Name; //this will be used for the directory name
        public List<VhdlOp> Operators;
        public List<VhdlTemplateBase> VhdlTemplates;
        public List<int> InputSizes;
        public string Part;
        public List<DataTypeFromSizeDelegate> DataTypes;
        public string VivadoPath;
        public bool DebugMode;
        public abstract void Populate();
        public TimingTestConfigBase() { Populate(); }
    }

    class TimingTestConfig : TimingTestConfigBase
    {
        public override void Populate()
        {
            Operators = new List<VhdlOp>
            {
                new VhdlOp(">",     "gt",   VhdlOp.ComparisonWithBoolOutput),
                new VhdlOp("<",     "lt",   VhdlOp.ComparisonWithBoolOutput),
                new VhdlOp(">=",    "ge",   VhdlOp.ComparisonWithBoolOutput),
                new VhdlOp("<=",    "le",   VhdlOp.ComparisonWithBoolOutput),
                new VhdlOp("=",     "eq",   VhdlOp.ComparisonWithBoolOutput),
                new VhdlOp("/=",    "neq",  VhdlOp.ComparisonWithBoolOutput),
                new VhdlOp("+",     "add",  VhdlOp.SameOutputDataType),
                new VhdlOp("-",     "sub",  VhdlOp.SameOutputDataType),
                new VhdlOp("/",     "div",  VhdlOp.SameOutputDataType),
                new VhdlOp("*",     "mul",  VhdlOp.DoubleSizedOutput),
                new VhdlOp("mod",   "mod",  VhdlOp.SameOutputDataType),
            };
            InputSizes = new List<int> { 32 };
            DataTypes = new List<DataTypeFromSizeDelegate> {
                (size, getFriendlyName) => { return (getFriendlyName) ? String.Format("unsigned{0}", size) : String.Format("unsigned({0} downto 0)", size-1); },
                (size, getFriendlyName) => { return (getFriendlyName) ? String.Format("signed{0}", size) : String.Format("signed({0} downto 0)", size-1); }
            };
            Part = "xc7a100tcsg324-1";
            VhdlTemplates = new List<VhdlTemplateBase> { new VhdlTemplateSync(), new VhdlTemplateAsync() };
            VivadoPath = "C:\\Xilinx\\Vivado\\2016.2\\bin\\vivado.bat";
            Name = "default";
            DebugMode = true;
        }

        int VhdlOpSizeMul(int inputSize) { return 2 * inputSize; } //multiplication needs 2 times wider output than input
    }

    class TimingTester
    {
        private TimingOutputParser Parser = new TimingOutputParser();
        private TimingTestConfigBase Test;

        const string TclTemplate = @"
read_vhdl UUT.vhd
read_xdc Constraints.xdc
synth_design -part %PART% -top tf_sample
config_timing_analysis -disable_flight_delays true
report_timing -file TimingReport.txt
report_timing_summary -check_timing_verbose -file TimingSummary.txt
show_schematic [get_nets]
write_schematic -force -format pdf -orientation landscape Schematic.pdf
quit
#
#opt_design
#place_design
#route_design";

        string CurrentTestOutputBaseDirectory;
        string CurrentTestOutputDirectory;

        public void InitializeTest(TimingTestConfigBase test)
        {
            Test = test;
            if (Directory.Exists("VivadoFiles")) Directory.Delete("VivadoFiles", true); //Clean the VivadoFiles directory (delete it recursively and mkdir)
            Directory.CreateDirectory("VivadoFiles");
            File.WriteAllText("VivadoFiles\\Generate.tcl", TclTemplate.Replace("%PART%", Test.Part));
            if (!Directory.Exists("TestResults")) Directory.CreateDirectory("TestResults");
            string currentTestDirectoryName = DateTime.Now.ToString("yyyy-MM-dd__hh-mm-ss")+"__"+Test.Name;
            CurrentTestOutputBaseDirectory = "TestResults\\"+currentTestDirectoryName;
            if(Directory.Exists(CurrentTestOutputBaseDirectory))
            { Logger.Log("Fatal error: the test directory already exists ({0}), which is very unlikely" +
                "because we used the date and time to generate the directory name.", CurrentTestOutputBaseDirectory); return; }
            Directory.CreateDirectory(CurrentTestOutputBaseDirectory);
            Logger.Init(CurrentTestOutputBaseDirectory+"\\Log.txt");
            RunTest();
        }

        string RunVivado(string vivadoPath, string tclFile)
        {
            Process cp = new Process();
            cp.StartInfo.FileName = vivadoPath;
            cp.StartInfo.Arguments = " -source " + tclFile;
            cp.StartInfo.WorkingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\VivadoFiles";
            //Logger.Log("WorkingDirectory = " + cp.StartInfo.WorkingDirectory);
            cp.StartInfo.UseShellExecute = true;
            cp.StartInfo.CreateNoWindow = false;
            //cp.StartInfo.RedirectStandardOutput = true;
            cp.Start();
            cp.WaitForExit();
            //return cp.StandardOutput.ReadToEnd();
            return "";
        }

        void CopyFileToOutputDir(string inputPath)
        {
            File.Copy(inputPath, CurrentTestOutputDirectory+"\\"+Path.GetFileName(inputPath));
        }

        void RunTest()
        {
            foreach (VhdlOp op in Test.Operators)
                foreach (int inputSize in Test.InputSizes)
                    foreach (TimingTestConfigBase.DataTypeFromSizeDelegate inputDataTypeFunction in Test.DataTypes)
                        foreach (VhdlTemplateBase myVhdlTemplate in Test.VhdlTemplates)
                        {
                            try
                            {
                                Logger.Log("========================== starting test ==========================");
                                string inputDataType = inputDataTypeFunction(inputSize, false);
                                string outputDataType = op.OutputDataTypeFunction(inputSize, inputDataTypeFunction, false);

                                string uutPath = "VivadoFiles\\UUT.vhd";
                                string xdcPath = "VivadoFiles\\Constraints.xdc";
                                string timingReportOutputPath = "VivadoFiles\\TimingReport.txt";
                                string timingSummaryOutputPath = "VivadoFiles\\TimingSummary.txt";
                                string schematicOutputPath = "VivadoFiles\\Schematic.pdf";

                                Logger.Log("Now generating: {0}({1}), {2}, {3} to {4}", op.FriendlyName, op.VhdlString, inputSize, inputDataType, outputDataType);
                                string testFriendlyName = String.Format("{0}_{1}_to_{2}_{3}",
                                    op.FriendlyName,
                                    inputDataTypeFunction(inputSize, true),
                                    op.OutputDataTypeFunction(inputSize, inputDataTypeFunction, true),
                                    myVhdlTemplate.Name);
                                    //friendly name should contain something from each "foreach" iterator
                                CurrentTestOutputDirectory = CurrentTestOutputBaseDirectory + "\\" + testFriendlyName;
                                Directory.CreateDirectory(CurrentTestOutputDirectory);
                                Logger.Log("\tDir name: {0}", testFriendlyName);

                                string vhdl = myVhdlTemplate.Template
                                    .Replace("%INTYPE%", inputDataType)
                                    .Replace("%OUTTYPE%", outputDataType)
                                    .Replace("%OPERATOR%", op.VhdlString);
                                File.WriteAllText(uutPath, vhdl);
                                CopyFileToOutputDir(uutPath);
                                File.WriteAllText(xdcPath, myVhdlTemplate.Xdc);
                                CopyFileToOutputDir(xdcPath);

                                Logger.LogInline("Running Vivado... ");
                                RunVivado(Test.VivadoPath, "Generate.tcl");
                                Logger.Log("Done.");
                                CopyFileToOutputDir(timingReportOutputPath);
                                CopyFileToOutputDir(timingSummaryOutputPath);
                                CopyFileToOutputDir(schematicOutputPath);
                                VivadoResult myVivadoResult = new VivadoResult();
                                myVivadoResult.TimingReport = File.ReadAllText(timingReportOutputPath);
                                myVivadoResult.TimingSummary = File.ReadAllText(timingSummaryOutputPath);
                                Parser.Parse(myVivadoResult);
                                Parser.PrintParsedTimingReport();
                                Parser.PrintParsedTimingSummary();
                                //return;
                            }
                            catch(Exception myException)
                            {
                                if (Test.DebugMode) throw;
                                else Logger.Log("Exception happened during test: {0}", myException.Message);
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
            myTimingTester.InitializeTest(test);
        }
    }

    static class Logger
    {
        private static StreamWriter LogStreamWriter;
        private static bool Initialized;

        static public void Init(string path)
        {
            LogStreamWriter = new StreamWriter(File.Create(path));
            LogStreamWriter.AutoFlush = true;
            Initialized = true;
        }

        static public void Log(string Format, params object[] Objs)
        {
            if(Initialized) LogStreamWriter.WriteLine(Format, Objs);
            Console.WriteLine(Format, Objs);
        }

        static public void LogInline(string Format, params object[] Objs)
        {
            if(Initialized) LogStreamWriter.WriteLine(Format, Objs);
            Console.WriteLine(Format, Objs);
        }
    }
}
