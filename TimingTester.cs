using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace HastlayerTimingTester
{
    /// <summary>This is for passing the data output by Vivado into TimingOutputParser.</summary>
    public struct VivadoResult
    {
        public string TimingReport;
        public string TimingSummary;
    }

    /// <summary>This class implements the core functionality of the Hastlayer Timing Tester application.</summary>
    class TimingTester
    {
        private TimingTestConfigBase _testConfig;

        /// <summary>This is like: @"TestResults\2016-09-15__10-52-19__default"</summary>
        public const string CurrentTestBaseDirectory = "CurrentTest";

        /// <summary>This is like: @"TestResults\2016-09-15__10-52-19__default\gt_unsigned32_to_boolean_comb"</summary>
        string CurrentTestOutputDirectory;

        enum TaskChoice { Prepare, Analyze }

        void PrepareAnalyze(TaskChoice taskChoice)
        {
            string taskChoiceString = (taskChoice == TaskChoice.Prepare) ? "prepare" : "analyze";
            Logger.Log("=== HastlayerTimingTester {0} stage ===", taskChoiceString);
            //var currentTestDirectoryName = DateTime.Now.ToString("yyyy-MM-dd__HH-mm-ss") + "__" + _testConfig.Name;

            StreamWriter batchWriter = null, resultsWriter = null;
            if (taskChoice == TaskChoice.Prepare)
            {
                batchWriter = new StreamWriter(File.Open(CurrentTestBaseDirectory + "\\Run.bat", FileMode.Create));
                batchWriter.AutoFlush = true;
                _testConfig.Driver.InitPrepare(batchWriter);
            }
            else
            {
                resultsWriter = new StreamWriter(File.Open(CurrentTestBaseDirectory + "\\Results.tsv", FileMode.Create));
                resultsWriter.AutoFlush = true;
                _testConfig.Driver.InitPrepare(resultsWriter);
            }

            foreach (var op in _testConfig.Operators)
            {
                foreach (var inputSize in _testConfig.InputSizes)
                {
                    foreach (var inputDataTypeFunction in op.DataTypes)
                    {
                        foreach (var vhdlTemplate in op.VhdlTemplates)
                        {
                            try
                            {
                                var inputDataType = inputDataTypeFunction(inputSize, false);
                                var outputDataType = op.OutputDataTypeFunction(
                                    inputSize,
                                    inputDataTypeFunction,
                                    false);

                                var testFriendlyName = string.Format("{0}_{1}_to_{2}_{3}",
                                    op.FriendlyName,
                                    inputDataTypeFunction(inputSize, true),
                                    op.OutputDataTypeFunction(inputSize, inputDataTypeFunction, true),
                                    vhdlTemplate.Name);
                                CurrentTestOutputDirectory = CurrentTestBaseDirectory + "\\" + testFriendlyName;
                                Directory.CreateDirectory(CurrentTestOutputDirectory);

                                Logger.Log("Now generating: {0}({1}), {2}, {3} to {4}", op.FriendlyName, op.VhdlString,
                                    inputSize, inputDataType, outputDataType);
                                Logger.Log("\tDir name: {0}", testFriendlyName);

                                if (taskChoice == TaskChoice.Prepare)
                                {
                                    var vhdl = vhdlTemplate.VhdlTemplate
                                        .Replace("%INTYPE%", inputDataType)
                                        .Replace("%OUTTYPE%", outputDataType)
                                        .Replace("%OPERATOR%", op.VhdlString);
                                    _testConfig.Driver.Prepare(testFriendlyName, vhdl, vhdlTemplate);
                                }
                                else
                                {
                                    var synthesisParser = _testConfig.Driver.Analyze(testFriendlyName, StaPhase.Synthesis);
                                    Logger.Log("Synthesis:\r\n----------");
                                    synthesisParser.PrintParsedTimingReport("S");
                                    synthesisParser.PrintParsedTimingSummary();
                                    var dataPathDelay = synthesisParser.DataPathDelay;
                                    var timingWindowDiffFromRequirement = synthesisParser.TimingWindowDiffFromRequirement;
                                    var useImplementationResults = false;

                                    if (_testConfig.ImplementDesign)
                                    {
                                        var implementationParser = _testConfig.Driver.Analyze(testFriendlyName,
                                            StaPhase.Implementation);
                                        if (implementationParser != null)
                                        {
                                            Logger.Log("Implementation:\r\n---------------");
                                            implementationParser.PrintParsedTimingReport("I");
                                            implementationParser.PrintParsedTimingSummary();
                                            useImplementationResults =
                                                dataPathDelay + timingWindowDiffFromRequirement <
                                                implementationParser.DataPathDelay +
                                                implementationParser.TimingWindowDiffFromRequirement;
                                            if (useImplementationResults)
                                            {
                                                Logger.Log("Chosen to use implementation results.");
                                                dataPathDelay = implementationParser.DataPathDelay;
                                                timingWindowDiffFromRequirement = 
                                                    implementationParser.TimingWindowDiffFromRequirement;
                                            }
                                            else
                                            {
                                                Logger.Log("Chosen to skip implementation results.");
                                            }
                                        }
                                    }
                                    resultsWriter.FormattedWriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}",
                                        op.VhdlString,
                                        inputDataTypeFunction(inputSize, true),
                                        op.OutputDataTypeFunction(inputSize, inputDataTypeFunction, true),
                                        vhdlTemplate.Name,
                                        (useImplementationResults) ? "impl" : "synth",
                                        dataPathDelay,
                                        timingWindowDiffFromRequirement
                                    );
                                }
                            }
                            catch (Exception exception)
                            {
                                if (_testConfig.DebugMode) throw;
                                else Logger.Log("Exception happened during prepare: {0}", exception.Message);
                            }
                        }
                    }
                }
            }
            batchWriter.Close();
        }



        /// <summary>
        /// This function gets things ready before the test, then runs the test.
        /// It creates the necessary directory structure, cleans up VivadoFiles and generates
        /// a Tcl script for Vivado.
        /// </summary>
        public void DoTest(TimingTestConfigBase testConfig, ProgramOptions options)
        {
            //Commandline options
            if (options.All) options.Analyze = options.ExecSta = options.Prepare = true;
            if (options.AllRemoteSta)
            {
                options.Analyze = options.Prepare = true;
                options.ExecSta = false;
            }

            _testConfig = testConfig;
            if (!Directory.Exists(CurrentTestBaseDirectory)) Directory.CreateDirectory(CurrentTestBaseDirectory);
            else if (Directory.GetFileSystemEntries(CurrentTestBaseDirectory).Length > 0)
            {
                Directory.Delete(CurrentTestBaseDirectory, true);
                Directory.CreateDirectory(CurrentTestBaseDirectory);
            }
            Logger.Init(CurrentTestBaseDirectory + "\\Log.txt", !options.Prepare);
            _testConfig.Driver.BaseDir = CurrentTestBaseDirectory;

            if (options.Prepare) PrepareAnalyze(TaskChoice.Prepare);
            if (options.ExecSta) ExecSta();
            if (options.AllRemoteSta)
            {
                Console.WriteLine(String.Format("Waiting for user to run tests and overwrite the result at {0}",
                    "TODO"));
                Console.ReadKey();
            }
            if (options.Analyze) PrepareAnalyze(TaskChoice.Analyze);
        }

        public void RunBatchFile(string path)
        {
            var cp = new Process();
            cp.StartInfo.WorkingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
                "\\" + TimingTester.CurrentTestBaseDirectory;
            cp.StartInfo.FileName = cp.StartInfo.WorkingDirectory + "\\Run.bat";
            cp.StartInfo.UseShellExecute = false;
            cp.StartInfo.CreateNoWindow = false;
            cp.StartInfo.RedirectStandardOutput = false;
            cp.Start();
            cp.WaitForExit();
            return;
        }


        public void ExecSta()
        {
            RunBatchFile(TimingTester.CurrentTestBaseDirectory + "\\Run.bat");
        }

    }


    /// <summary>Copies the given file from VivadoFiles to the output directory of the current test.</summary>
    /*
    void CopyFileToOutputDir(string inputPath)
    {
        File.Copy(inputPath, CurrentTestOutputDirectory + "\\" + Path.GetFileName(inputPath));
    }
    */

}
