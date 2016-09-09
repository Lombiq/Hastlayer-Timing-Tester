﻿using System;
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
        public List<string> vhdlTemplates;
        public List<int> inputSizes;
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
                (x, y) => { return (y) ? "unsigned" : String.Format("unsigned({0} downto 0)", x-1); },
                (x, y) => { return (y) ? "signed" : String.Format("signed({0} downto 0)", x-1); }
            };
            part = "xc7a100tcsg324-1";
            vhdlTemplates = new List<string> { "sync", "async" };
            vivadoPath = "C:\\Xilinx\\Vivado\\2016.2\\bin\\vivado.bat";
            name = "default";
        }

        int vhdlOpSizeMul(int input_size) { return 2*input_size; } //multiplication needs 2 times wider output than input
    }

    class TimingTester
    {
        TimingTestConfigBase test;

        const string vhdlTemplate =
@"library ieee;
use ieee.std_logic_1164.all;
use ieee.numeric_std.all;

entity tf_sample is
port(
    a1      : in %INTYPE%;
    a2      : in %INTYPE%;
    aout    : out %OUTTYPE%
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

        string currentTestDirectory;

        public void initializeTest(TimingTestConfigBase _test)
        {
            test = _test;
            if (Directory.Exists("VivadoFiles")) Directory.Delete("VivadoFiles", true); //Clean the VivadoFiles directory (delete it recursively and mkdir)
            Directory.CreateDirectory("VivadoFiles");
            File.WriteAllText("VivadoFiles\\Generate.tcl", tclTemplate.Replace("%PART%", test.part));
            if (!Directory.Exists("TestResults")) Directory.CreateDirectory("TestResults");
            string currentTestDirectoryName = DateTime.Now.ToString("yyyy-MM-dd__hh-mm-ss")+"__"+test.name;
            currentTestDirectory = "TestResults\\"+currentTestDirectoryName;
            if(Directory.Exists(currentTestDirectory)) { Console.WriteLine("The test directory already exists: ", currentTestDirectory); return; }
            Directory.CreateDirectory(currentTestDirectory);
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

        void runTest()
        {
            foreach (string vhdlTemplateName in test.vhdlTemplates)
                foreach (vhdlOp op in test.operators)
                    foreach (int inputSize in test.inputSizes)
                        foreach (TimingTestConfigBase.dataTypeFromSize inputDataTypeFunction in test.dataTypes)
                        {
                            Console.WriteLine("========================== starting test ==========================");
                            string inputDataType = inputDataTypeFunction(inputSize, false);
                            string outputDataType = op.outputDataTypeFunction(inputSize, inputDataTypeFunction, false);
                            string uutPath = "VivadoFiles\\UUT.vhd";
                            string timingOutputPath = "VivadoFiles\\timing.txt";
                            Console.WriteLine("Now generating: {0}({1}), {2}, {3} to {4}", op.friendlyName, op.vhdlString, inputSize, inputDataType, outputDataType);
                            string testFriendlyName = String.Format("{0}_{1}_{2}to{3}", op.friendlyName, inputDataTypeFunction(0, true), inputSize, op.outputDataTypeFunction(inputSize, inputDataTypeFunction, false));
                            string testOutputPath = currentTestDirectory + "\\" + testFriendlyName;
                            Directory.CreateDirectory(testOutputPath);
                            Console.WriteLine("\tDir name: {0}", testFriendlyName);
                            string vhdl = vhdlTemplate
                                .Replace("%INTYPE%", inputDataType)
                                .Replace("%OUTTYPE%", outputDataType)
                                .Replace("%OPERATOR%", op.vhdlString);
                            File.WriteAllText(uutPath, vhdl);
                            File.Copy(uutPath, testOutputPath+"\\UUT.vhd");
                            Console.Write("Running Vivado... ");
                            runVivado(test.vivadoPath, "Generate.tcl");
                            Console.WriteLine("Done.");
                            File.Copy(timingOutputPath, testOutputPath+"\\timing.txt");
                            string timingData = File.ReadAllText(timingOutputPath);
                            string dataPathDelayLine = "";
                            Regex.Split(timingData, "\r\n").ToList().ForEach((x) => { if (x.Contains("Data Path Delay")) dataPathDelayLine = x; });
                            string tempLinePart = Regex.Split(dataPathDelayLine, "\\(logic")[0];
                            tempLinePart = Regex.Split(tempLinePart, "Data Path Delay:")[1];
                            tempLinePart = Regex.Split(tempLinePart, "ns")[0];
                            tempLinePart = tempLinePart.Trim();
                            float dataPathDelay = float.Parse(tempLinePart, CultureInfo.InvariantCulture);
                            Console.WriteLine("Data path delay = {0} ns;  Max clock frequency = {1} MHz", dataPathDelay, Math.Floor((1 / (dataPathDelay * 1e-9)) / 1000) / 1000);
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
            myTimingTester.initializeTest(test);
        }
    }
}


//function cleanFiles() //TODO
//{
//
//}
