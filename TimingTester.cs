using HastlayerTimingTester.Parsers;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace HastlayerTimingTester
{

    /// <summary>Implements the core functionality of the Hastlayer Timing Tester application.</summary>
    class TimingTester
    {
        private TimingTestConfigBase _testConfig;

        /// <summary>The output directory of all tests to be done when running the Timing Tester.</summary>
        public const string CurrentTestBaseDirectory = "CurrentTest";

        /// <summary>The output directory of the actual test being done when running the Timing Tester.
        /// This is like: @"CurrentTests\gt_unsigned32_to_boolean_comb"</summary>
        string CurrentTestOutputDirectory;

        enum TaskChoice { Prepare, Analyze }

        /// <summary>
        /// Implements the --prepare and the --analyze stages of processing.
        /// </summary>
        /// <param name="taskChoice">This parameter allows us to choose which stage to do.</param>
        void PrepareAnalyze(TaskChoice taskChoice)
        {
            var taskChoiceString = (taskChoice == TaskChoice.Prepare) ? "prepare" : "analyze";
            Logger.LogStageHeader(taskChoiceString);

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
                resultsWriter.Write("Op\tInType\tOutType\tTemplate\tDesignStat\tDPD\tTWDFR\r\n");
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


                                Logger.Log("\r\nCurrent test item: {0}, {1}, {2} to {3}", op.FriendlyName, inputSize,
                                    inputDataType, outputDataType);
                                if (!op.VhdlExpression.IsValid(inputSize, inputDataTypeFunction, vhdlTemplate))
                                {
                                    Logger.Log("This test item was skipped due to not considered valid.");
                                    continue;
                                }

                                CurrentTestOutputDirectory = CurrentTestBaseDirectory + "\\" + testFriendlyName;
                                Directory.CreateDirectory(CurrentTestOutputDirectory);
                                Logger.Log("\tDir name: {0}", testFriendlyName);

                                if (taskChoice == TaskChoice.Prepare)
                                {
                                    var vhdl = AdditionalVhdlIncludes.Content + vhdlTemplate.VhdlTemplate
                                        .Replace("%INTYPE%", inputDataType)
                                        .Replace("%OUTTYPE%", outputDataType)
                                        .Replace("%EXPRESSION%",
                                            op.VhdlExpression.GetVhdlCode(vhdlTemplate.ExpressionInputs, inputSize));
                                    _testConfig.Driver.Prepare(testFriendlyName, vhdl, vhdlTemplate);
                                }
                                else // if taskChoice == TaskChoice.Analyze
                                {
                                    decimal? dataPathDelay = null, timingWindowDiffFromRequirement = null;
                                    var useImplementationResults = false;

                                    if (!_testConfig.ImplementDesign &&
                                        !_testConfig.Driver.CanStaAfterSynthesize &&
                                        _testConfig.Driver.CanStaAfterImplementation)
                                        throw new Exception("Can't STA after synthesize step for this FPGA vendor, " +
                                            "although ImplementDesign is false in the config.");

                                    if (_testConfig.ImplementDesign && !_testConfig.Driver.CanStaAfterImplementation)
                                        throw new Exception("Can't STA after implementation step for this FPGA vendor, " +
                                            "although ImplementDesign is true in the config.");

                                    if (_testConfig.Driver.CanStaAfterSynthesize)
                                    {
                                        var synthesisParser = _testConfig.Driver.Analyze(
                                            testFriendlyName, StaPhase.Synthesis);
                                        Logger.Log("\r\nSynthesis:\r\n----------");
                                        synthesisParser.PrintParsedTimingReport("S");
                                        synthesisParser.PrintParsedTimingSummary();
                                        dataPathDelay = synthesisParser.DataPathDelay;
                                        timingWindowDiffFromRequirement = synthesisParser.TimingWindowDiffFromRequirement;
                                    }

                                    if (_testConfig.Driver.CanStaAfterImplementation && _testConfig.ImplementDesign)
                                    {
                                        var implementationParser = _testConfig.Driver.Analyze(testFriendlyName,
                                            StaPhase.Implementation);
                                        if (implementationParser != null)
                                        {
                                            Logger.Log("\r\nImplementation:\r\n---------------");
                                            implementationParser.PrintParsedTimingReport("I");
                                            implementationParser.PrintParsedTimingSummary();
                                            useImplementationResults =
                                                dataPathDelay == null ||
                                                timingWindowDiffFromRequirement == null ||
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

                                    if (dataPathDelay == null || timingWindowDiffFromRequirement == null)
                                    {
                                        Logger.Log("Error: couldn't acquire valid timing value from " +
                                            "neither synthesis, nor implementation.");
                                    }

                                    resultsWriter.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}",
                                        op.FriendlyName,
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
                                else Logger.Log("Exception happened during {0}: {1}",
                                    taskChoiceString, exception.Message);
                            }
                        }
                    }
                }
            }
            if (batchWriter != null)
            {
                batchWriter.WriteLine("echo ===== Finished =====");
                batchWriter.Close();
            }
            if (resultsWriter != null) resultsWriter.Close();
        }

        /// <summary>
        /// Gets things ready before the test, then runs the test.
        /// </summary>
        public void DoTests(TimingTestConfigBase testConfig, ProgramOptions options)
        {
            // Command-line options
            if (options.All) options.Analyze = options.ExecSta = options.Prepare = true;
            if (options.AllRemoteSta)
            {
                options.Analyze = options.Prepare = true;
                options.ExecSta = false;
            }

            _testConfig = testConfig;
            if (options.Prepare)
            {
                if (!Directory.Exists(CurrentTestBaseDirectory)) Directory.CreateDirectory(CurrentTestBaseDirectory);
                else if (Directory.GetFileSystemEntries(CurrentTestBaseDirectory).Length > 0)
                {
                    Directory.Delete(CurrentTestBaseDirectory, true);
                    Directory.CreateDirectory(CurrentTestBaseDirectory);
                }
            }
            Logger.Init(CurrentTestBaseDirectory + "\\Log.txt", options.Prepare);
            _testConfig.Driver.BaseDir = CurrentTestBaseDirectory;

            if (options.Prepare) PrepareAnalyze(TaskChoice.Prepare);
            if (options.ExecSta) ExecSta();
            if (options.AllRemoteSta)
            {
                Console.WriteLine(string.Format("\r\nWaiting for user to run tests and overwrite the result in {0}\r\n",
                    CurrentTestBaseDirectory));
                Console.ReadKey();
            }
            if (options.Analyze) PrepareAnalyze(TaskChoice.Analyze);
        }

        /// <summary>
        /// Runs the Run.bat in CurrentTest to apply STA.
        /// </summary>
        public void ExecSta()
        {
            Logger.LogStageHeader("execute-sta");
            using (var cp = new Process())
            {
                cp.StartInfo.WorkingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
                    "\\" + CurrentTestBaseDirectory;
                cp.StartInfo.FileName = cp.StartInfo.WorkingDirectory + "\\Run.bat";
                cp.StartInfo.UseShellExecute = false;
                cp.StartInfo.CreateNoWindow = false;
                cp.StartInfo.RedirectStandardOutput = false;
                cp.Start();
                cp.WaitForExit();
            }
        }
    }
}
