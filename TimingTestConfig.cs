
using System;
using System.Collections.Generic;

namespace HastlayerTimingTester
{
    class TimingTestConfig : TimingTestConfigBase
    {
        public TimingTestConfig()
        {
            Operators = new List<VhdlOp>
                {
                    //The list of operators where VhdlOp is like:
                    //  new VhdlOp(string vhdlString, string friendlyName, OutputDataTypeDelegate outputDataTypeFunction)
                    //      vhdlString is the part actually substituted into the VHDL template while testing the operator.
                    //  friendlyName is the name used in the results: logs, directory names, etc. This is required as
                    //      directory names cannot contain some special characters that the vhdlString has.
                    //  outputDataTypeFunction generates the output data type for the operator.
                    //      There are some predefined functions for that, see class vhdlOp.
                    //      VhdlOp.SameOutputDataType should be the default; it means that the output of the operator is the same as its input.
                    //      VhdlOp.ComparisonWithBoolOutput is used for comparisons, which e.g. can use numbers as their input, but they output a true/false value.
                    //      VhdlOp.DoubleSizedOutput is useful for multiplication, where e.g. we need an unsigned(63 downto 0) output if the operands are unsigned(31 downto 0).
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
            InputSizes = new List<int> { 128, 64, 32, 16, 8 }; //The list of input sizes for the data type that we want to test
            DataTypes = new List<DataTypeFromSizeDelegate> {
                //A list of functions that can generate the input data types that we test.
                //For example, for an input size of 32, we should get unsigned(31 downto 0) to be pasted into the VHDL template.
                //However, if a friendly name is requested instead, "unsigned32" is returned, which can be safely used in directory names.
                    (size, getFriendlyName) => { return (getFriendlyName) ? String.Format("unsigned{0}", size) : String.Format("unsigned({0} downto 0)", size-1); },
                    (size, getFriendlyName) => { return (getFriendlyName) ? String.Format("signed{0}", size) : String.Format("signed({0} downto 0)", size-1); }
                };
            Part = "xc7a100tcsg324-1"; //The FPGA part number
            VhdlTemplates = new List<VhdlTemplateBase> { new VhdlTemplateSync(), new VhdlTemplateAsync() }; //The VHDL templates that will be used for analysis
            Frequency = 100e6F; //System clock frequency in MHz
            Name = "default"; //Name of the configuration, will be used in the name of the output directory
            VivadoPath = "C:\\Xilinx\\Vivado\\2016.2\\bin\\vivado.bat"; //The path where vivado.bat is located
            DebugMode = true; //If DebugMode is true, the Hastlayer Timing Tester will stop at any exceptions during tests.
                              //If it is false, the exceptions is logged and the program continues with the next test.
        }
    }
}
