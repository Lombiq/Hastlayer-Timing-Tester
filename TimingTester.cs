using HastlayerTimingTester.Parsers;
using HastlayerTimingTester.Vhdl;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HastlayerTimingTester
{
    /// <summary>Implements the core functionality of the Hastlayer Timing Tester application.</summary>
    internal class TimingTester
    {
        private TimingTestConfigBase _testConfig;

        /// <summary>The output directory of all tests to be done when running the Timing Tester.</summary>
        private const string CurrentTestBaseDirectory = "CurrentTest";


        /// <summary>
        /// Gets things ready before the test, then runs the test.
        /// </summary>
        public void DoTests(TimingTestConfigBase testConfig, ProgramParameters parameters)
        {
            // Command-line parameters.
            if (parameters.All) parameters.Analyze = parameters.ExecSta = parameters.Prepare = true;
            if (parameters.AllRemoteSta)
            {
                parameters.Analyze = parameters.Prepare = true;
                parameters.ExecSta = false;
            }

            _testConfig = testConfig;
            if (parameters.Prepare)
            {
                if (!Directory.Exists(CurrentTestBaseDirectory)) Directory.CreateDirectory(CurrentTestBaseDirectory);
                else if (Directory.GetFileSystemEntries(CurrentTestBaseDirectory).Length > 0)
                {
                    Directory.Delete(CurrentTestBaseDirectory, true);
                    Directory.CreateDirectory(CurrentTestBaseDirectory);
                }
            }
            Logger.Init(CombineWithBaseDirectoryPath("Log.txt"), parameters.Prepare);

            if (parameters.Prepare) PrepareAnalyze(TaskChoice.Prepare);
            if (parameters.ExecSta) ExecSta();
            if (parameters.AllRemoteSta)
            {
                Console.WriteLine($"\r\nWaiting for user to run tests and overwrite the result in {CurrentTestBaseDirectory}.\r\n");
                Console.ReadKey();
            }
            if (parameters.Analyze) PrepareAnalyze(TaskChoice.Analyze);

            Logger.Dispose();
        }

        /// <summary>
        /// Runs the Run.bat script(s) in CurrentTest to apply STA.
        /// </summary>
        public void ExecSta()
        {
            Logger.LogStageHeader("execute-sta");

            var tasks = new Task[_testConfig.NumberOfSTAProcesses];

            for (int i = 0; i < _testConfig.NumberOfSTAProcesses; i++)
            {
                tasks[i] = Task.Factory.StartNew(indexObject =>
                {
                    var folder = Path.Combine(
                            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                            CurrentTestBaseDirectory,
                            GetProcessFolderName((int)indexObject));

                    if (!Directory.Exists(folder)) return;

                    using (var process = new Process())
                    {
                        process.StartInfo.WorkingDirectory = folder;
                        process.StartInfo.FileName = Path.Combine(process.StartInfo.WorkingDirectory, "Run.bat");
                        process.StartInfo.UseShellExecute = _testConfig.NumberOfSTAProcesses > 1;
                        process.StartInfo.CreateNoWindow = false;
                        process.StartInfo.RedirectStandardOutput = false;
                        process.Start();
                        process.WaitForExit();
                    }
                }, i);
            }

            Task.WaitAll(tasks);
            Logger.LogStageFooter("execute-sta");
        }


        /// <summary>
        /// Implements the --prepare and the --analyze stages of processing.
        /// </summary>
        /// <param name="taskChoice">This parameter allows us to choose which stage to do.</param>
        private void PrepareAnalyze(TaskChoice taskChoice)
        {
            var taskChoiceString = (taskChoice == TaskChoice.Prepare) ? "prepare" : "analyze";
            Logger.LogStageHeader(taskChoiceString);

            StreamWriter batchWriter = null, resultsWriter = null;

            void CreateBatchWriter(string processFolderPath)
            {
                Directory.CreateDirectory(processFolderPath);

                batchWriter?.WriteLine("echo ===== Finished =====");
                batchWriter?.Dispose();
                batchWriter = new StreamWriter(File.Open(Path.Combine(processFolderPath, "Run.bat"), FileMode.Create))
                {
                    AutoFlush = true
                };
                batchWriter.WriteLine("mode con lines=99999");
                _testConfig.Driver.InitPrepare(batchWriter);
            }

            if (taskChoice == TaskChoice.Analyze)
            {
                resultsWriter = new StreamWriter(File.Open(CombineWithBaseDirectoryPath("Results.tsv"), FileMode.Create))
                {
                    AutoFlush = true
                };
                resultsWriter.Write("Op\tInType\tOutType\tTemplate\tDesignStat\tDPD\tTWDFR\r\n");
            }

            var testCount = 0;
            ExecuteForOperators((op, inputSize, inputDataTypeFunction, vhdlTemplate) =>
            {
                if (op.VhdlExpression.IsValid(inputSize, inputDataTypeFunction, vhdlTemplate))
                {
                    testCount++;
                }
            });

            var actualNumberOfSTAProcesses = testCount < _testConfig.NumberOfSTAProcesses ? testCount : _testConfig.NumberOfSTAProcesses;

            // Creating script to be able to easily execute all STA scripts in parallel by hand too.
            var scriptBuilder = new StringBuilder();

            for (int i = 0; i < actualNumberOfSTAProcesses; i++)
            {
                scriptBuilder.AppendLine("cd " + i);
                scriptBuilder.AppendLine("start cmd /c Run.bat");
                scriptBuilder.AppendLine("cd ..");
            }
            File.WriteAllText(CombineWithBaseDirectoryPath("Run.bat"), scriptBuilder.ToString());

            try
            {
                // The last process will have all the remainder tests.
                var testsPerProcess = testCount / actualNumberOfSTAProcesses;
                var lastProcessIndex = actualNumberOfSTAProcesses - 1;
                var testIndex = 0;
                var isLastProcess = false;
                var testIndexInCurrentProcess = 0;
                var testsPerCurrentProcess = testsPerProcess;
                var previousProcessIndex = -1;

                ExecuteForOperators((op, inputSize, inputDataTypeFunction, vhdlTemplate) =>
                {
                    try
                    {
                        var processIndex = (int)Math.Floor((double)testIndex / testsPerProcess);
                        if (processIndex >= actualNumberOfSTAProcesses) processIndex = lastProcessIndex;
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
                            return;
                        }

                        if (!isLastProcess && processIndex != previousProcessIndex)
                        {
                            previousProcessIndex = processIndex;
                            testIndexInCurrentProcess = 0;
                            var processFolderPath = CombineWithBaseDirectoryPath(GetProcessFolderName(processIndex));

                            _testConfig.Driver.CurrentRootDirectoryPath = processFolderPath;

                            if (taskChoice == TaskChoice.Prepare)
                            {
                                CreateBatchWriter(processFolderPath);
                            }

                            if (processIndex == lastProcessIndex)
                            {
                                isLastProcess = true;
                                testsPerCurrentProcess = testCount - testIndex;
                            }
                        }

                        if (taskChoice == TaskChoice.Prepare)
                        {
                            Directory.CreateDirectory(CombineWithBaseDirectoryPath(GetProcessFolderName(processIndex), testFriendlyName));
                            Logger.Log("\tDir name: {0}", testFriendlyName);

                            batchWriter.WriteLine(
                                $"{Environment.NewLine}echo STARTING #{testIndexInCurrentProcess} / {testsPerCurrentProcess} at {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}{Environment.NewLine}");

                            var vhdl = AdditionalVhdlIncludes.Content + vhdlTemplate.VhdlTemplate
                                .Replace("%INTYPE%", inputDataType)
                                .Replace("%OUTTYPE%", outputDataType)
                                .Replace("%EXPRESSION%",
                                    op.VhdlExpression.GetVhdlCode(vhdlTemplate.ExpressionInputs, inputSize));
                            _testConfig.Driver.Prepare(testFriendlyName, vhdl, vhdlTemplate);


                            batchWriter.WriteLine(
                                $"echo {Environment.NewLine}echo FINISHED #{testIndexInCurrentProcess} / {testsPerCurrentProcess} at {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}{Environment.NewLine}");
                        }
                        else // if taskChoice == TaskChoice.Analyze
                        {
                            decimal? dataPathDelay = null, timingWindowDiffFromRequirement = null;
                            var useImplementationResults = false;

                            if (!_testConfig.ImplementDesign &&
                                !_testConfig.Driver.CanStaAfterSynthesize &&
                                _testConfig.Driver.CanStaAfterImplementation)
                            {
                                throw new Exception(
                                    "Can't STA after synthesize step for this FPGA vendor, " +
                                    "although ImplementDesign is false in the config.");
                            }

                            if (_testConfig.ImplementDesign && !_testConfig.Driver.CanStaAfterImplementation)
                            {
                                throw new Exception(
                                    "Can't STA after implementation step for this FPGA vendor, " +
                                    "although ImplementDesign is true in the config.");
                            }

                            if (_testConfig.Driver.CanStaAfterSynthesize)
                            {
                                var synthesisParser = _testConfig.Driver.Analyze(testFriendlyName, StaPhase.Synthesis);
                                Logger.Log("\r\nSynthesis:\r\n----------");
                                synthesisParser.PrintParsedTimingReport("S");
                                synthesisParser.PrintParsedTimingSummary();
                                dataPathDelay = synthesisParser.DataPathDelay;
                                timingWindowDiffFromRequirement = synthesisParser.TimingWindowDiffFromRequirement;
                            }

                            if (_testConfig.Driver.CanStaAfterImplementation && _testConfig.ImplementDesign)
                            {
                                var implementationParser = _testConfig.Driver.Analyze(
                                    testFriendlyName,
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
                        else Logger.Log("Exception happened during {0}: {1}", taskChoiceString, exception.Message);
                    }
                    finally
                    {
                        testIndex++;
                        testIndexInCurrentProcess++;
                    }
                });
            }
            finally
            {
                if (batchWriter != null)
                {
                    batchWriter.WriteLine("echo ===== Finished =====");
                    batchWriter.Dispose();
                }
                if (resultsWriter != null) resultsWriter.Dispose();
            }

            Logger.LogStageFooter(taskChoiceString);
        }


        private string CombineWithBaseDirectoryPath(params string[] subPaths) =>
            Path.Combine(new[] { CurrentTestBaseDirectory }.Union(subPaths).ToArray());

        private string GetProcessFolderName(int processIndex) =>
            _testConfig.NumberOfSTAProcesses < 2 ? string.Empty : processIndex.ToString();

        private void ExecuteForOperators(Action<VhdlOp, int, VhdlOp.DataTypeFromSizeDelegate, VhdlTemplateBase> processor)
        {
            foreach (var op in _testConfig.Operators)
                foreach (var inputSize in _testConfig.InputSizes)
                    foreach (var inputDataTypeFunction in op.DataTypes)
                        foreach (var vhdlTemplate in op.VhdlTemplates)
                        {
                            processor(op, inputSize, inputDataTypeFunction, vhdlTemplate);
                        }
        }


        private enum TaskChoice { Prepare, Analyze }
    }
}
