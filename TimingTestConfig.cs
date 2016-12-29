
using System;
using System.Collections.Generic;

namespace HastlayerTimingTester
{
    /// <summary>
    /// Provides configuration for Hastlayer Timing Tester.
    /// You need to edit the source and recompile the application to change configuration.
    /// See the comments in the source of this class for detailed description of the options.
    /// It is advised to check <see cref="Operators"> first.
    /// </summary>
    class TimingTestConfig : TimingTestConfigBase
    {
        public TimingTestConfig()
        {
            // There are lists of functions below that can generate input data types to test.
            // Any of these can be used as a parameter to the constructor of VhdlOp.
            // (It is advised to start with the Operators variable when looking at this file.)
            // For example, for an input size of 32, we should get "unsigned(31 downto 0)" to be pasted into the VHDL
            // template. However, if a friendly name is requested instead, "unsigned32" is returned, which can be safely
            // used in directory names.
            List<VhdlOp.DataTypeFromSizeDelegate> SUNumericDataTypes = new List<VhdlOp.DataTypeFromSizeDelegate> {
                (size, getFriendlyName) =>
                {
                    return (getFriendlyName) ?
                        string.Format("unsigned{0}", size) :
                        string.Format("unsigned({0} downto 0)", size-1);
                },
                (size, getFriendlyName) =>
                {
                    return (getFriendlyName) ?
                        string.Format("signed{0}", size) :
                        string.Format("signed({0} downto 0)", size-1);
                }
            };
            List<VhdlOp.DataTypeFromSizeDelegate> StdLogicVectorDataType = new List<VhdlOp.DataTypeFromSizeDelegate> {
                (size, getFriendlyName) =>
                {
                    return (getFriendlyName) ?
                    string.Format("std_logic_vector{0}", size) :
                    string.Format("std_logic_vector({0} downto 0)", size-1);
                }
            };

            // There are lists of VHDL templates below. Any of them can be used as a parameter to VhdlOp constructor.
            // (It is advised to start with the Operators variable when looking at this file.)
            List<VhdlTemplateBase> DefaultVhdlTemplates = new List<VhdlTemplateBase> { new VhdlTemplateSync() };
            List<VhdlTemplateBase> UnaryVhdlTemplates = new List<VhdlTemplateBase> { new VhdlTemplateSyncUnary() };

            // Operators is the list of operators where VhdlOp is like:
            //  new VhdlOp(string vhdlString, string friendlyName, OutputDataTypeDelegate outputDataTypeFunction)
            //      vhdlString is the part actually substituted into the VHDL template while testing the operator.
            //  friendlyName is the name used in the results: logs, directory names, etc. This is required as
            //      directory names cannot contain some special characters that the vhdlString has.
            //  outputDataTypeFunction generates the output data type for the operator.
            //      There are some predefined functions for that, see class vhdlOp.
            //      VhdlOp.SameOutputDataType should be the default; it means that the output of the operator is
            //          the same as its input.
            //      VhdlOp.ComparisonWithBoolOutput is used for comparisons, which e.g. can use numbers as their
            //          input, but they output a true/false value.
            //      VhdlOp.DoubleSizedOutput is useful for multiplication, where e.g. we need an
            //          unsigned(63 downto 0) output if the operands are unsigned(31 downto 0).
            Operators = new List<VhdlOp>
            {
                new VhdlOp("not",   "not",  StdLogicVectorDataType, VhdlOp.SameOutputDataType,        UnaryVhdlTemplates),
                new VhdlOp("and",   "and",  StdLogicVectorDataType, VhdlOp.SameOutputDataType,        DefaultVhdlTemplates),
                new VhdlOp("nand",  "nand", StdLogicVectorDataType, VhdlOp.SameOutputDataType,        DefaultVhdlTemplates),
                new VhdlOp("or",    "or",   StdLogicVectorDataType, VhdlOp.SameOutputDataType,        DefaultVhdlTemplates),
                new VhdlOp("nor",   "nor",  StdLogicVectorDataType, VhdlOp.SameOutputDataType,        DefaultVhdlTemplates),
                new VhdlOp("xor",   "xor",  StdLogicVectorDataType, VhdlOp.SameOutputDataType,        DefaultVhdlTemplates),
                new VhdlOp("xnor",  "xnor", StdLogicVectorDataType, VhdlOp.SameOutputDataType,        DefaultVhdlTemplates),
                new VhdlOp("+",     "add",  SUNumericDataTypes,     VhdlOp.SameOutputDataType,        DefaultVhdlTemplates),
                new VhdlOp(">",     "gt",   SUNumericDataTypes,     VhdlOp.ComparisonWithBoolOutput,  DefaultVhdlTemplates),
                new VhdlOp("<",     "lt",   SUNumericDataTypes,     VhdlOp.ComparisonWithBoolOutput,  DefaultVhdlTemplates),
                new VhdlOp(">=",    "ge",   SUNumericDataTypes,     VhdlOp.ComparisonWithBoolOutput,  DefaultVhdlTemplates),
                new VhdlOp("<=",    "le",   SUNumericDataTypes,     VhdlOp.ComparisonWithBoolOutput,  DefaultVhdlTemplates),
                new VhdlOp("=",     "eq",   SUNumericDataTypes,     VhdlOp.ComparisonWithBoolOutput,  DefaultVhdlTemplates),
                new VhdlOp("/=",    "neq",  SUNumericDataTypes,     VhdlOp.ComparisonWithBoolOutput,  DefaultVhdlTemplates),
                new VhdlOp("-",     "sub",  SUNumericDataTypes,     VhdlOp.SameOutputDataType,        DefaultVhdlTemplates),
                new VhdlOp("/",     "div",  SUNumericDataTypes,     VhdlOp.SameOutputDataType,        DefaultVhdlTemplates),
                new VhdlOp("*",     "mul",  SUNumericDataTypes,     VhdlOp.DoubleSizedOutput,         DefaultVhdlTemplates),
                new VhdlOp("mod",   "mod",  SUNumericDataTypes,     VhdlOp.SameOutputDataType,        DefaultVhdlTemplates),
            };

            // InputSizes is the list of input sizes for the data type that we want to test
            InputSizes = new List<int> { 128, 32, 64, 16, 8 };

            Part = "xc7a100tcsg324-1"; // The FPGA part number


            Frequency = 100e6m; // System clock frequency in MHz
            Name = "default"; // Name of the configuration, will be used in the name of the output directory
            VivadoPath = "C:\\Xilinx\\Vivado\\2016.2\\bin\\vivado.bat"; // The path where vivado.bat is located

            // If DebugMode is true, the Hastlayer Timing Tester will stop at any exceptions during tests.
            // If it is false, the exceptions are logged and the program continues with the next test.
            DebugMode = true;

            // If VivadoBatchMode is true, Vivado shares the console window of Hastlayer Timing Tester.
            // It does not open the GUI for every single test. However, it cannot generate schematic drawings
            // (Schematic.pdf).
            // Note: if you are using Vivado in GUI mode with VivadoBatchMode = false and with ImplementDesign = true,
            // only generate designs that are possible to implement, or a message box will pop up with Tcl errors,
            // and the tests will hang.
            VivadoBatchMode = true;

            // If ImplementDesign is true, Vivado will perform STA for both the synthesized and the implemented designs.
            // If it is false, Vivado will only do synthesis + STA, and skip implementation + STA.
            ImplementDesign = true;

            // If DryRun is true, Vivado will not ne ran, only the tests to be taken will be logged.
            // The VivadoFiles directory will still be cleaned, and the UUT will be generated for all test cases.
            DryRun = false;
        }
    }
}
