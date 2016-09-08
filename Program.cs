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
    class vhdlOp
    {
        public static int sizeNoChange(int size) { return size; }
        public delegate int outputSizeFromInputSize(int size);
        public string vhdlString;
        public string friendlyName;
        public outputSizeFromInputSize outputSizeFunction;
        public vhdlOp(string vhdlString, string friendlyName, outputSizeFromInputSize outputSizeFunction)
            { this.vhdlString = vhdlString; this.friendlyName = friendlyName; this.outputSizeFunction = outputSizeFunction; }
    }

    abstract class TimingTestConfigBase
    {
        public delegate string dataTypeFromSize(int size);

        public List<vhdlOp> operators;
        public List<string> operatorNames;
        public List<string> vhdlTemplates;
        public List<int> sizes;
        public string part;
        public List<dataTypeFromSize> dataTypes;
        public string vivadoPath;
        public abstract void Populate();
        public TimingTestConfigBase() { Populate(); }
    }

    class TimingTestConfig : TimingTestConfigBase
    {
        public override void Populate()
        {
            operators = new List<vhdlOp>
            {
                new vhdlOp("+",     "add",  vhdlOp.sizeNoChange),
                new vhdlOp("-",     "sub",  vhdlOp.sizeNoChange),
                new vhdlOp("/",     "div",  vhdlOp.sizeNoChange),
                new vhdlOp("*",     "mul",  vhdlOp.sizeNoChange),
                new vhdlOp("mod",   "mod",  vhdlOp.sizeNoChange),
                new vhdlOp(">",     "gz",   vhdlOp.sizeNoChange),
                new vhdlOp("<",     "lt",   vhdlOp.sizeNoChange),
                new vhdlOp(">=",    "ge",   vhdlOp.sizeNoChange),
                new vhdlOp("<=",    "le",   vhdlOp.sizeNoChange),
                new vhdlOp("=",     "eq",   vhdlOp.sizeNoChange),
                new vhdlOp("/=",    "neq",  vhdlOp.sizeNoChange)
            };
            sizes = new List<int> { 32 };
            dataTypes = new List<dataTypeFromSize> { (x) => { return String.Format("unsigned({0} downto 0)", x-1); } };
            part = "xc7a100tcsg324-1";
            vhdlTemplates = new List<string> { "sync", "async" };
            vivadoPath = "C:\\Xilinx\\Vivado\\2016.2\\bin\\vivado.bat";
        }
    }

    class TimingTester
    {
        TimingTestConfigBase test;

        const string vhdlTemplate = @"
library ieee;
    use ieee.std_logic_1164.all;
    use ieee.numeric_std.all;

    entity tf_sample is
port(
    a1      : in %TYPE%;
    a2      : in %TYPE%;
    aout    : out %TYPE%
);
end tf_sample;

    architecture imp of tf_sample is begin
    aout <= a1 %OPERATOR% a2;
end imp;";

        const string tclTemplate = @"
read_vhdl UUT.vhd
synth_design -part %PART% -top tf_sample
config_timing_analysis -disable_flight_delays true
report_timing -file Timing.txt
quit
#
#report_timing_summary -delay_type min_max -report_unconstrained -check_timing_verbose -max_paths 10 -input_pins -name timing_1
#opt_design
#place_design
#route_design";

        public void Run(TimingTestConfigBase _test)
        {
            test = _test;
            //getVivadoPath((x)=>{
            //    Console.WriteLine("Vivado detected at "+(vivadoPath=x));
            //Console.WriteLine(fs.existsSync("VivadoFiles"));
            if (!Directory.Exists("VivadoFiles")) Directory.CreateDirectory("VivadoFiles");
            File.WriteAllText("VivadoFiles\\Generate.tcl", tclTemplate.Replace("%PART%", test.part));
            runTests();
            //});

        }

        string runVivado(string vivadoPath, string tclFile)
        {
            Process cp = new Process();
            cp.StartInfo.FileName = vivadoPath;
            cp.StartInfo.Arguments = " -source " + tclFile;
            cp.StartInfo.WorkingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\VivadoFiles";
            Console.WriteLine("WorkingDirectory = " + cp.StartInfo.WorkingDirectory);
            cp.StartInfo.UseShellExecute = true;
            cp.StartInfo.CreateNoWindow = false;
            //cp.StartInfo.RedirectStandardOutput = true;
            cp.Start();
            cp.WaitForExit();
            //return cp.StandardOutput.ReadToEnd();
            return "";
        }

        void runTests()
        {
            foreach (string vhdlTemplateName in test.vhdlTemplates)
                foreach (vhdlOp op in test.operators)
                    foreach (int size in test.sizes)
                        foreach (TimingTestConfigBase.dataTypeFromSize dataTypeFun in test.dataTypes)
                        {
                            var dataType = dataTypeFun(size);
                            Console.WriteLine("Now generating: {0}, {1}, {2}", op.friendlyName, size, dataType);
                            string vhdl = vhdlTemplate.Replace("%TYPE%", dataType).Replace("%OPERATOR%", op.vhdlString);
                            File.WriteAllText("VivadoFiles\\UUT.vhd", vhdl);
                            //Console.WriteLine("Now testing:\r\n==========================\r\n"+vhdl+"\r\n==========================");
                            Console.WriteLine("Running Vivado...");
                            runVivado(test.vivadoPath, "Generate.tcl");
                            Console.WriteLine("done.");
                            string timingData = File.ReadAllText("VivadoFiles\\timing.txt");
                            string dataPathDelayLine = "";
                            Regex.Split(timingData, "\r\n").ToList().ForEach((x) => { if (x.Contains("Data Path Delay")) dataPathDelayLine = x; });
                            string tempLinePart = Regex.Split(dataPathDelayLine, "\\(logic")[0];
                            tempLinePart = Regex.Split(tempLinePart, "Data Path Delay:")[1];
                            tempLinePart = Regex.Split(tempLinePart, "ns")[0];
                            tempLinePart = tempLinePart.Trim();
                            float dataPathDelay = float.Parse(tempLinePart, CultureInfo.InvariantCulture);
                            Console.WriteLine("data path delay = {0} ns;  max clock frequency = {1} MHz", dataPathDelay, Math.Floor((1 / (dataPathDelay * 1e-9)) / 1000) / 1000);
                            //return;
                        }
        }
   }



    class Program
    {
        static void Main(string[] args)
        {
            TimingTester myTimingTester = new TimingTester();
            TimingTestConfigBase test = new TimingTestConfig();
            myTimingTester.Run(test);
        }
    }
}


//function cleanFiles() //TODO
//{
//    fs.readdirSync(".").forEach((x) => { if (x.endsWith(".backup.jou") && x.startsWith("vivado")) fs.unlinkSync("./" + x); });
//}
