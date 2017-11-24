using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace HastlayerTimingTester
{
    /// <summary>This is for passing the data output by Vivado into TimingOutputParser.</summary>
    struct VivadoResult
    {
        public string TimingReport;
        public string TimingSummary;
    }

    /// <summary>This class implements the core functionality of the Hastlayer Timing Tester application.</summary>
    class TimingTester
    {
        private TimingOutputParser _parser;
        private TimingTestConfigBase _testConfig;

        /// <summary>This is like: @"TestResults\2016-09-15__10-52-19__default"</summary>
        public const string CurrentTestBaseDirectory = "CurrentTest";

        /// <summary>This is like: @"TestResults\2016-09-15__10-52-19__default\gt_unsigned32_to_boolean_comb"</summary>
        string CurrentTestOutputDirectory;

        void Prepare()
        {
            Logger.Log("=== HastlayerTimingTester Prepare stage ===");
            //var currentTestDirectoryName = DateTime.Now.ToString("yyyy-MM-dd__HH-mm-ss") + "__" + _testConfig.Name;
            _testConfig.Driver.InitPrepare();

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

                                _testConfig.Driver.Prepare(testFriendlyName, op, inputSize, inputDataType, outputDataType, vhdlTemplate);
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

        }

        public void ExecSta()
        {

        }

        public void Analyze()
        {
            /*
                Logger.WriteResult("Op\tInType\tOutType\tTemplate\tDesignStat\tDPD\tTWD\r\n");

                Logger.Log("Starting analysis at: {0}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                RunTest();
                Logger.Log("Analysis finished at: {0}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                */
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
            _parser = new TimingOutputParser(testConfig.Frequency);
            if (!Directory.Exists(CurrentTestBaseDirectory)) Directory.CreateDirectory(CurrentTestBaseDirectory);
            else if (Directory.GetFileSystemEntries(CurrentTestBaseDirectory).Length > 0)
            {
                Directory.Delete(CurrentTestBaseDirectory, true);
                Directory.CreateDirectory(CurrentTestBaseDirectory);
            }
            Logger.Init(CurrentTestBaseDirectory + "\\Log.txt", !options.Prepare);
            _testConfig.Driver.BaseDir = CurrentTestBaseDirectory;

            if (options.Prepare) Prepare();
            if (options.ExecSta) ExecSta();
            if (options.AllRemoteSta)
            {
                Console.WriteLine(String.Format("Waiting for user to run tests and overwrite the result at {0}",
                    "TODO"));
                Console.ReadKey();
            }
            if (options.Analyze) Analyze();
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
