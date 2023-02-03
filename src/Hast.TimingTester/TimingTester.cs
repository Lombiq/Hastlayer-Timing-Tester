using HastlayerTimingTester.Parsers;
using HastlayerTimingTester.Vhdl;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HastlayerTimingTester;

/// <summary>
/// Implements the core functionality of the Hastlayer Timing Tester application.
/// </summary>
internal class TimingTester
{
    private readonly TimingTestConfig _testConfig;

    /// <summary>
    /// Gets the output directory of all tests to be done when running the Timing Tester.
    /// </summary>
    public string CurrentTestBaseDirectory => _testConfig.Name;

    public TimingTester(TimingTestConfig testConfig) => _testConfig = testConfig;

    /// <summary>
    /// Gets things ready before the test, then runs the test.
    /// </summary>
    public void DoTests(ProgramParameters parameters)
    {
        // Command-line parameters.
        if (parameters.All) parameters.Analyze = parameters.ExecSta = parameters.Prepare = true;
        if (parameters.AllRemoteSta)
        {
            parameters.Analyze = parameters.Prepare = true;
            parameters.ExecSta = false;
        }

        if (parameters.Prepare)
        {
            if (!Directory.Exists(CurrentTestBaseDirectory))
            {
                Directory.CreateDirectory(CurrentTestBaseDirectory);
            }
            else if (Directory.GetFileSystemEntries(CurrentTestBaseDirectory).Length > 0)
            {
                Directory.Delete(CurrentTestBaseDirectory, true);
                Directory.CreateDirectory(CurrentTestBaseDirectory);
            }
        }

        Logger.Init(CombineWithBaseDirectoryPath("Log.txt"), parameters.Prepare);

        if (parameters.Prepare) PrepareAndAnalyze(TaskChoice.Prepare);
        if (parameters.ExecSta) ExecSta();
        if (parameters.AllRemoteSta)
        {
            Console.WriteLine(
                $"{Environment.NewLine}Waiting for user to run tests and overwrite the result in {CurrentTestBaseDirectory}.{Environment.NewLine}");
            Console.ReadKey();
        }

        if (parameters.Analyze) PrepareAndAnalyze(TaskChoice.Analyze);

        Logger.Close();
    }

    /// <summary>
    /// Runs the Run.bat script(s) in CurrentTest to apply STA.
    /// </summary>
    private void ExecSta()
    {
        Logger.LogStageHeader("execute-sta");

        var tasks = new Task[_testConfig.NumberOfStaProcesses];

        for (int i = 0; i < _testConfig.NumberOfStaProcesses; i++)
        {
            tasks[i] = Task.Factory.StartNew(
                indexObject =>
                {
                    var folder = Path.Combine(
                        // We need the actual executing assembly.
#pragma warning disable S3902 // "Assembly.GetExecutingAssembly" should not be called
                        Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
#pragma warning restore S3902 // "Assembly.GetExecutingAssembly" should not be called
                        CurrentTestBaseDirectory,
                        GetProcessFolderName((int)indexObject));

                    if (!Directory.Exists(folder)) return;

                    using var process = new Process();
                    process.StartInfo.WorkingDirectory = folder;
                    process.StartInfo.FileName = Path.Combine(process.StartInfo.WorkingDirectory, "Run.bat");
                    process.StartInfo.UseShellExecute = _testConfig.NumberOfStaProcesses > 1;
                    process.StartInfo.CreateNoWindow = false;
                    process.StartInfo.RedirectStandardOutput = false;
                    process.Start();
                    process.WaitForExit();
                },
                i,
                CancellationToken.None,
                TaskCreationOptions.None,
                TaskScheduler.Default);
        }

        Task.WaitAll(tasks);
        Logger.LogStageFooter("execute-sta");
    }

    /// <summary>
    /// Implements the --prepare and the --analyze stages of processing.
    /// </summary>
    /// <param name="taskChoice">This parameter allows us to choose which stage to do.</param>
    [SuppressMessage(
        "Critical Code Smell",
        "S3776:Cognitive Complexity of methods should not be too high",
        Justification = "Refactoring deferred to a later time.")]
    [SuppressMessage(
        "Critical Code Smell",
        "S134:Control flow statements \"if\", \"switch\", \"for\", \"foreach\", \"while\", \"do\"  and \"try\" should not be nested too deeply",
        Justification = "Refactoring deferred to a later time.")]
    private void PrepareAndAnalyze(TaskChoice taskChoice)
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
                AutoFlush = true,
            };
            // Increasing the command buffer size. 32766 is the maximum (see:
            // https://stackoverflow.com/questions/4692673/how-to-change-screen-buffer-size-in-windows-command-prompt-from-batch-script)
            batchWriter.WriteLine("mode con lines=32766");
            _testConfig.Driver.InitPrepare(batchWriter);
        }

        if (taskChoice == TaskChoice.Analyze)
        {
            resultsWriter = new StreamWriter(File.Open(CombineWithBaseDirectoryPath("Results.tsv"), FileMode.Create))
            {
                AutoFlush = true,
            };
            resultsWriter.Write($"Op\tInType\tOutType\tTemplate\tDesignStat\tDPD\tTWDFR{Environment.NewLine}");
        }

        var testCount = 0;
        ExecuteForOperators((op, inputSize, inputDataTypeFunction, vhdlTemplate) =>
        {
            if (op.VhdlExpression.IsValid(inputSize, inputDataTypeFunction, vhdlTemplate))
            {
                testCount++;
            }
        });

        var actualNumberOfSTAProcesses = testCount < _testConfig.NumberOfStaProcesses ? testCount : _testConfig.NumberOfStaProcesses;

        var scriptBuilder = new StringBuilder();

        // Creating script to be able to easily execute all STA scripts in parallel by hand too.
        for (int i = 0; i < actualNumberOfSTAProcesses; i++)
        {
            scriptBuilder.AppendLine("cd " + i.ToTechnicalString());
            scriptBuilder.AppendLine("start cmd /c Run.bat");
            scriptBuilder.AppendLine("cd ..");
        }

        File.WriteAllText(CombineWithBaseDirectoryPath("Run.bat"), scriptBuilder.ToString());

        // Creating script to be able to easily tail all STA processes' progress logs.
        scriptBuilder.Clear();

        scriptBuilder.AppendLine(CultureInfo.InvariantCulture, $@"Set-ItemProperty -Path 'HKLM:\SOFTWARE\Wow6432Node\Microsoft\.NetFramework\v4.0.30319' -Name 'SchUseStrongCrypto' -Value '1' -Type DWord");
        scriptBuilder.AppendLine(@"Set-ItemProperty -Path 'HKLM:\SOFTWARE\Microsoft\.NetFramework\v4.0.30319' -Name 'SchUseStrongCrypto' -Value '1' -Type DWord");
        scriptBuilder.AppendLine("Install-PackageProvider -Name NuGet -MinimumVersion 2.8.5.201 -Force");
        // For details on Gridify see:
        // http://ridicurious.com/2017/11/14/set-gridlayout-arrange-apps-and-scripts-in-an-automatic-grid-to-fit-your-screen/
        scriptBuilder.AppendLine("Install-Module Gridify -scope CurrentUser -Confirm:$False -Force");

        for (int i = 0; i < actualNumberOfSTAProcesses; i++)
        {
            scriptBuilder.AppendLine("Invoke-Expression 'cmd /c start powershell -NoExit -Command {");
            scriptBuilder.AppendLine(CultureInfo.InvariantCulture, $"    $host.UI.RawUI.WindowTitle = \"Tailing the #{i} progress log\";");
            scriptBuilder.AppendLine(CultureInfo.InvariantCulture, $"    Get-Content {i.ToTechnicalString()}{Path.DirectorySeparatorChar}Progress.log -Wait;");
            scriptBuilder.AppendLine("}';");
            scriptBuilder.AppendLine();
        }

        scriptBuilder.AppendLine("Do");
        scriptBuilder.AppendLine("{");
        scriptBuilder.AppendLine("    $processes = (Get-Process | Where-Object { $_.MainWindowTitle -like \"Tailing the * progress log\" })");
        scriptBuilder.AppendLine("}");
        scriptBuilder.AppendLine(CultureInfo.InvariantCulture, $"Until ($processes.Length -eq {actualNumberOfSTAProcesses})");
        scriptBuilder.AppendLine("Set-GridLayout -Process $processes");

        File.WriteAllText(CombineWithBaseDirectoryPath("Tail.ps1"), scriptBuilder.ToString());

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

                    var testFriendlyName =
                        $"{op.FriendlyName}_{inputDataTypeFunction(inputSize, true)}_to_{op.OutputDataTypeFunction(inputSize, inputDataTypeFunction, true)}_{vhdlTemplate.Name}";

                    Logger.Log(Environment.NewLine + "Current test item: {0}, {1}, {2} to {3}", op.FriendlyName, inputSize, inputDataType, outputDataType);

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
                            $"{Environment.NewLine}echo STARTING #{testIndexInCurrentProcess.ToTechnicalString()} / " +
                            $"{testsPerCurrentProcess.ToTechnicalString()} at %date% %time% >> Progress.log{Environment.NewLine}");

                        var vhdl = AdditionalVhdlIncludes.Content + vhdlTemplate.VhdlTemplate
                            .Replace("%INTYPE%", inputDataType, StringComparison.InvariantCulture)
                            .Replace("%OUTTYPE%", outputDataType, StringComparison.InvariantCulture)
                            .Replace("%EXPRESSION%", op.VhdlExpression.GetVhdlCode(vhdlTemplate.ExpressionInputs, inputSize), StringComparison.InvariantCulture);
                        _testConfig.Driver.Prepare(testFriendlyName, vhdl, vhdlTemplate);

                        batchWriter.WriteLine(
                            $"{Environment.NewLine}echo FINISHED #{testIndexInCurrentProcess.ToTechnicalString()} / " +
                            $"{testsPerCurrentProcess.ToTechnicalString()} at %date% %time% >> Progress.log{Environment.NewLine}");
                    }
                    else if (taskChoice == TaskChoice.Analyze)
                    {
                        decimal? dataPathDelay = null, timingWindowDiffFromRequirement = null;
                        var useImplementationResults = false;

                        if (!_testConfig.ImplementDesign &&
                            !_testConfig.Driver.CanStaAfterSynthesize &&
                            _testConfig.Driver.CanStaAfterImplementation)
                        {
                            throw new NotSupportedException(
                                "Can't STA after synthesize step for this FPGA vendor, " +
                                "although ImplementDesign is false in the config.");
                        }

                        if (_testConfig.ImplementDesign && !_testConfig.Driver.CanStaAfterImplementation)
                        {
                            throw new NotSupportedException(
                                "Can't STA after implementation step for this FPGA vendor, " +
                                "although ImplementDesign is true in the config.");
                        }

                        if (_testConfig.Driver.CanStaAfterSynthesize)
                        {
                            var synthesisParser = _testConfig.Driver.Analyze(testFriendlyName, StaPhase.Synthesis);
                            Logger.Log($"{Environment.NewLine}Synthesis:{Environment.NewLine}----------");
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
                                Logger.Log($"{Environment.NewLine}Implementation:{Environment.NewLine}---------------");
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

                        resultsWriter.WriteLine(
                            "{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}",
                            op.FriendlyName,
                            inputDataTypeFunction(inputSize, true),
                            op.OutputDataTypeFunction(inputSize, inputDataTypeFunction, true),
                            vhdlTemplate.Name,
                            useImplementationResults ? "impl" : "synth",
                            dataPathDelay,
                            timingWindowDiffFromRequirement
                        );
                    }

                    // These incrementation are here and in the catch, not in a finally because we don't want to run
                    // them if an early return happens (when the operation would be invalid).
                    testIndex++;
                    testIndexInCurrentProcess++;
                }
                catch (Exception exception)
                {
                    testIndex++;
                    testIndexInCurrentProcess++;

                    if (_testConfig.DebugMode) throw;
                    else Logger.Log("Exception happened during {0}: {1}", taskChoiceString, exception.Message);
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
        _testConfig.NumberOfStaProcesses < 2 ? string.Empty : processIndex.ToString(CultureInfo.InvariantCulture);

    private void ExecuteForOperators(Action<VhdlOp, int, VhdlOp.DataTypeFromSizeDelegate, VhdlTemplateBase> processor)
    {
        foreach (var (op, inputSize) in _testConfig.Operators.SelectMany(op => _testConfig.InputSizes.Select(inputSize => (op, inputSize))))
        {
            var inner = op.DataTypes.SelectMany(inputDataTypeFunction => op.VhdlTemplates.Select(vhdlTemplate => (inputDataTypeFunction, vhdlTemplate)));
            foreach (var (inputDataTypeFunction, vhdlTemplate) in inner)
            {
                processor(op, inputSize, inputDataTypeFunction, vhdlTemplate);
            }
        }
    }

    private enum TaskChoice
    {
        Prepare,
        Analyze,
    }
}
