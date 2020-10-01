using HastlayerTimingTester.Drivers;
using HastlayerTimingTester.Vhdl;
using HastlayerTimingTester.Vhdl.Expressions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;

namespace HastlayerTimingTester
{
    /// <summary>
    /// Provides configuration for Hastlayer Timing Tester.
    /// </summary>
    /// <remarks>
    /// <para>
    /// You need to edit the source and recompile the application to change configuration. See the comments in the
    /// source of this class for detailed description of the options. It is advised to check <see cref="Operators"/>
    /// first.
    /// </para>
    /// </remarks>
    internal class TimingTestConfig
    {
        public string Name { get; protected set; }
        public List<VhdlOp> Operators { get; private set; }
        public List<int> InputSizes { get; private set; }
        public string Part { get; protected set; }
        public string VivadoPath { get; protected set; }
        public bool DebugMode { get; protected set; }
        public decimal FrequencyHz { get; protected set; }
        public bool VivadoBatchMode { get; protected set; }
        public bool ImplementDesign { get; protected set; }
        public int NumberOfThreadsPerProcess { get; protected set; }
        public int NumberOfStaProcesses { get; protected set; }
        public FpgaVendorDriver Driver { get; protected set; }


        public TimingTestConfig()
        {
            // There are lists of functions below that can generate input data types to test.
            // Any of these can be used as a parameter to the constructor of VhdlOp.
            // (It is advised to start with the Operators variable when looking at this file.)
            // For example, for an input size of 32, we should get "unsigned(31 downto 0)" to be pasted into the VHDL
            // template. However, if a friendly name is requested instead, "unsigned32" is returned, which can be
            // safely used in directory names.
            var suNumericDataTypes = new List<VhdlOp.DataTypeFromSizeDelegate>
            {
                (size, getFriendlyName) => getFriendlyName ? $"unsigned{size}" : $"unsigned({size - 1} downto 0)",
                (size, getFriendlyName) => getFriendlyName ? $"signed{size}" : $"signed({size - 1} downto 0)",
            };

            var stdLogicVectorDataType = new List<VhdlOp.DataTypeFromSizeDelegate>
            {
                (size, getFriendlyName) => getFriendlyName ? $"std_logic_vector{size}" : $"std_logic_vector({size - 1} downto 0)",
            };

            // There are lists of VHDL templates below. Any of them can be used as a parameter to VhdlOp constructor.
            // (It is advised to start with the Operators variable when looking at this file.)
            var defaultVhdlTemplates = new List<VhdlTemplateBase> { new VhdlTemplateSync() };

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
                new VhdlOp(new BinaryOperatorVhdlExpression("and"),   "and",  stdLogicVectorDataType, VhdlOp.SameOutputDataType,        defaultVhdlTemplates),
                new VhdlOp(new BinaryOperatorVhdlExpression("nand"),  "nand", stdLogicVectorDataType, VhdlOp.SameOutputDataType,        defaultVhdlTemplates),
                new VhdlOp(new BinaryOperatorVhdlExpression("or"),    "or",   stdLogicVectorDataType, VhdlOp.SameOutputDataType,        defaultVhdlTemplates),
                new VhdlOp(new BinaryOperatorVhdlExpression("nor"),   "nor",  stdLogicVectorDataType, VhdlOp.SameOutputDataType,        defaultVhdlTemplates),
                new VhdlOp(new BinaryOperatorVhdlExpression("xor"),   "xor",  stdLogicVectorDataType, VhdlOp.SameOutputDataType,        defaultVhdlTemplates),
                new VhdlOp(new BinaryOperatorVhdlExpression("xnor"),  "xnor", stdLogicVectorDataType, VhdlOp.SameOutputDataType,        defaultVhdlTemplates),
                new VhdlOp(new BinaryOperatorVhdlExpression("+"),     "add",  suNumericDataTypes,     VhdlOp.SameOutputDataType,        defaultVhdlTemplates),
                new VhdlOp(new BinaryOperatorVhdlExpression(">"),     "gt",   suNumericDataTypes,     VhdlOp.ComparisonWithBoolOutput,  defaultVhdlTemplates),
                new VhdlOp(new BinaryOperatorVhdlExpression("<"),     "lt",   suNumericDataTypes,     VhdlOp.ComparisonWithBoolOutput,  defaultVhdlTemplates),
                new VhdlOp(new BinaryOperatorVhdlExpression(">="),    "ge",   suNumericDataTypes,     VhdlOp.ComparisonWithBoolOutput,  defaultVhdlTemplates),
                new VhdlOp(new BinaryOperatorVhdlExpression("<="),    "le",   suNumericDataTypes,     VhdlOp.ComparisonWithBoolOutput,  defaultVhdlTemplates),
                new VhdlOp(new BinaryOperatorVhdlExpression("="),     "eq",   suNumericDataTypes,     VhdlOp.ComparisonWithBoolOutput,  defaultVhdlTemplates),
                new VhdlOp(new BinaryOperatorVhdlExpression("/="),    "neq",  suNumericDataTypes,     VhdlOp.ComparisonWithBoolOutput,  defaultVhdlTemplates),
                new VhdlOp(new BinaryOperatorVhdlExpression("-"),     "sub",  suNumericDataTypes,     VhdlOp.SameOutputDataType,        defaultVhdlTemplates),
                new VhdlOp(new BinaryOperatorVhdlExpression("mod"),   "mod",  suNumericDataTypes,     VhdlOp.SameOutputDataType,        defaultVhdlTemplates),
                new VhdlOp(new BinaryOperatorVhdlExpression("rem"),   "rem",  suNumericDataTypes,     VhdlOp.SameOutputDataType,        defaultVhdlTemplates),
                new VhdlOp(new UnaryOperatorVhdlExpression("not"),    "not",  stdLogicVectorDataType, VhdlOp.SameOutputDataType,        defaultVhdlTemplates),
                // Unary plus is a noop, not needed.
                ////new VhdlOp(
                ////    new UnaryOperatorVhdlExpression("+", UnaryOperatorVhdlExpression.ValidationMode.AnyDataType),
                ////    "unary_plus",
                ////    suNumericDataTypes,
                ////    VhdlOp.SameOutputDataType,
                ////    defaultVhdlTemplates),
                new VhdlOp(
                    new UnaryOperatorVhdlExpression("-", UnaryOperatorVhdlExpression.ValidationMode.SignedOnly),
                    "unary_minus",
                    suNumericDataTypes,
                    VhdlOp.SameOutputDataType,
                    defaultVhdlTemplates),
                new VhdlOp(
                    new WrapSmartResizeVhdlExpression(new BinaryOperatorVhdlExpression("/")),
                    "div",
                    suNumericDataTypes,
                    VhdlOp.SameOutputDataType,
                    defaultVhdlTemplates),
                new VhdlOp(
                    new WrapSmartResizeVhdlExpression(new BinaryOperatorVhdlExpression("*")),
                    "mul",
                    suNumericDataTypes,
                    VhdlOp.SameOutputDataType,
                    defaultVhdlTemplates),
            };

            // We test shifting by the amount of bits listed below.
            // Multiplying by a constant 2^N is also a shift operation, so we test that here, too. As we expect the
            // FPGA compiler to implement this by wiring, multiplying by 2^N is expected to be faster than multiplying
            // by another constant or another variable (where it would use DSP blocks).
            for (int i = 0; i < 64; i++)
            {
                // These are the original shift test cases, however the DotnetShiftVhdlExpression implements the
                // expression that Hastlayer really uses:
                ////Operators.Add(new VhdlOp(
                ////    new ShiftVhdlExpression(ShiftVhdlExpression.Direction.Left, i),
                ////    "shift_left_by_" + i.ToString(CultureInfo.InvariantCulture),
                ////    suNumericDataTypes,
                ////    VhdlOp.SameOutputDataType,
                ////    defaultVhdlTemplates));
                ////Operators.Add(new VhdlOp(
                ////    new ShiftVhdlExpression(ShiftVhdlExpression.Direction.Right, i),
                ////    "shift_right_by_" + i.ToString(CultureInfo.InvariantCulture),
                ////    suNumericDataTypes,
                ////    VhdlOp.SameOutputDataType,
                ////    defaultVhdlTemplates));

                Operators.Add(new VhdlOp(
                    new DotnetShiftVhdlExpression(DotnetShiftVhdlExpression.Direction.Left, 64, true, false, i),
                    "dotnet_shift_left_by_" + i.ToString(CultureInfo.InvariantCulture),
                    suNumericDataTypes,
                    VhdlOp.SameOutputDataType,
                    defaultVhdlTemplates));
                Operators.Add(new VhdlOp(
                    new DotnetShiftVhdlExpression(DotnetShiftVhdlExpression.Direction.Right, 64, true, false, i),
                    "dotnet_shift_right_by_" + i.ToString(CultureInfo.InvariantCulture),
                    suNumericDataTypes,
                    VhdlOp.SameOutputDataType,
                    defaultVhdlTemplates));
                Operators.Add(new VhdlOp(
                    new DotnetShiftVhdlExpression(DotnetShiftVhdlExpression.Direction.Left, 32, true, false, i),
                    "dotnet_shift_left_by_" + i.ToString(CultureInfo.InvariantCulture),
                    suNumericDataTypes,
                    VhdlOp.SameOutputDataType,
                    defaultVhdlTemplates));
                Operators.Add(new VhdlOp(
                    new DotnetShiftVhdlExpression(DotnetShiftVhdlExpression.Direction.Right, 32, true, false, i),
                    "dotnet_shift_right_by_" + i.ToString(CultureInfo.InvariantCulture),
                    suNumericDataTypes,
                    VhdlOp.SameOutputDataType,
                    defaultVhdlTemplates));

                // These are test cases for /(2^n) or *(2^n) which is practically just a shift:
                var powTwoOfI = BigInteger.Pow(2, i);
                Operators.Add(new VhdlOp(
                    new MutiplyDivideByConstantVhdlExpression(
                        powTwoOfI,
                        MutiplyDivideByConstantVhdlExpression.Mode.Multiply,
                        MutiplyDivideByConstantVhdlExpression.ValidationMode.UnsignedOnly),
                    "mul_by_" + powTwoOfI.ToString(CultureInfo.InvariantCulture),
                    suNumericDataTypes,
                    VhdlOp.SameOutputDataType,
                    defaultVhdlTemplates));
                Operators.Add(new VhdlOp(
                    new MutiplyDivideByConstantVhdlExpression(
                        powTwoOfI,
                        MutiplyDivideByConstantVhdlExpression.Mode.Multiply,
                        MutiplyDivideByConstantVhdlExpression.ValidationMode.SignedOnly),
                    "mul_by_" + powTwoOfI.ToString(CultureInfo.InvariantCulture),
                    suNumericDataTypes,
                    VhdlOp.SameOutputDataType,
                    defaultVhdlTemplates));
                Operators.Add(new VhdlOp(
                    new MutiplyDivideByConstantVhdlExpression(
                        powTwoOfI,
                        MutiplyDivideByConstantVhdlExpression.Mode.Divide,
                        MutiplyDivideByConstantVhdlExpression.ValidationMode.UnsignedOnly),
                    "div_by_" + powTwoOfI.ToString(CultureInfo.InvariantCulture),
                    suNumericDataTypes,
                    VhdlOp.SameOutputDataType,
                    defaultVhdlTemplates));
                Operators.Add(new VhdlOp(
                    new MutiplyDivideByConstantVhdlExpression(
                        powTwoOfI,
                        MutiplyDivideByConstantVhdlExpression.Mode.Divide,
                        MutiplyDivideByConstantVhdlExpression.ValidationMode.SignedOnly),
                    "div_by_" + powTwoOfI.ToString(CultureInfo.InvariantCulture),
                    suNumericDataTypes,
                    VhdlOp.SameOutputDataType,
                    defaultVhdlTemplates));
            }

            // These are DotnetShiftExpression with a<<b where both are variables:
            foreach (int outputSize in new List<int> { 32, 64 })
            {
                Operators.Add(new VhdlOp(
                        new DotnetShiftVhdlExpression(DotnetShiftVhdlExpression.Direction.Left, outputSize, false, false),
                        "dotnet_shift_left",
                        suNumericDataTypes,
                        VhdlOp.SameOutputDataType,
                        defaultVhdlTemplates));
                Operators.Add(new VhdlOp(
                        new DotnetShiftVhdlExpression(DotnetShiftVhdlExpression.Direction.Right, outputSize, false, false),
                        "dotnet_shift_right",
                        suNumericDataTypes,
                        VhdlOp.SameOutputDataType,
                        defaultVhdlTemplates));
            }

            // Just to test MultiplyDivideByConstantVhdlExpression behavior on constant 0 or negative numbers:
            Operators.Add(new VhdlOp(
                new MutiplyDivideByConstantVhdlExpression(
                    -2,
                    MutiplyDivideByConstantVhdlExpression.Mode.Multiply,
                    MutiplyDivideByConstantVhdlExpression.ValidationMode.SignedOnly),
                "mul_by_-2",
                suNumericDataTypes,
                VhdlOp.SameOutputDataType,
                defaultVhdlTemplates));
            Operators.Add(new VhdlOp(
                new MutiplyDivideByConstantVhdlExpression(
                    0,
                    MutiplyDivideByConstantVhdlExpression.Mode.Multiply,
                    MutiplyDivideByConstantVhdlExpression.ValidationMode.UnsignedOnly),
                "mul_by_0",
                suNumericDataTypes,
                VhdlOp.SameOutputDataType,
                defaultVhdlTemplates));


            // InputSizes is the list of input sizes for the data type that we want to test
            InputSizes = new List<int> { 1, 8, 16, 32, 64 };

            // The FPGA part name. Only used for Xilinx devices. You can find these in the Xilinx Board Store here:
            // https://github.com/Xilinx/XilinxBoardStore (if you can't find something there then check out the branch
            // for the given Vitis version, like this one for 2020.1.1:
            // https://github.com/Xilinx/XilinxBoardStore/tree/2020.1.1/boards/Xilinx. E.g. the part name for the Alveo
            // U280 board is in the
            // https://github.com/Xilinx/XilinxBoardStore/blob/master/boards/Xilinx/au280/production/1.1/board.xml
            // file, just search for "part_name". Be sure to use the production versions, not the engineering sample
            // ("es").
            // Use the existing configurations under the TimingTestConfigs folder instead of directly changing this
            // here, and create new configs for new boards.
            ////Part = "xc7a100tcsg324-1";

            // System clock frequency in Hz
            ////Frequency = 100e6m;

            // Name of the configuration, will be used in the name of the output directory
            Name = "default";

            // If DebugMode is true, the Hastlayer Timing Tester will stop at any exceptions during tests.
            // If it is false, the exceptions are logged and the program continues with the next test.
            DebugMode = false;

            // This selects for which FPGA vendor do we want to run the timing test.
            // XilinxDriver supports Vivado, IntelDriver supports Quartus and TimeQuest.
            ////Driver = new XilinxDriver(this, @"C:\Xilinx\Vivado\2016.4\bin\vivado.bat");

            // If VivadoBatchMode is true, Vivado shares the console window of Hastlayer Timing Tester. It does not
            // open the GUI for every single test. However, it cannot generate schematic drawings (Schematic.pdf).
            // Note: if you are using Vivado in GUI mode with VivadoBatchMode = false and with ImplementDesign = true,
            // only generate designs that are possible to implement, or a message box will pop up with Tcl errors, and
            // the tests will hang.
            VivadoBatchMode = true;

            // If ImplementDesign is true, Vivado will perform STA for both the synthesized and the implemented designs.
            // If it is false, Vivado will only do synthesis + STA, and skip implementation + STA.
            ImplementDesign = true;

            // Determines the number of threads used during simulation in a given FPGA vendor tool process if the tool
            // supports it. Currently only Vivado supports multi threading. This should most of the time correspond to
            // the number of logical processors (cores) in the system. Also see NumberOfSTAProcesses.
            NumberOfThreadsPerProcess = Environment.ProcessorCount;

            // Determines the number of FPGA vendor tool processes to use during static timing analysis. Since even
            // when supposedly using all the cores the FPGA tools can't usually utilize the whole CPU you can increase
            // the degree of parallelism further. Also see NumberOfThreadsPerProcess.
            NumberOfStaProcesses = 6;
        }
    }
}
