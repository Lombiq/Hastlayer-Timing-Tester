using HastlayerTimingTester.Drivers;
using HastlayerTimingTester.Vhdl;
using HastlayerTimingTester.Vhdl.Expressions;
using System.Collections.Generic;
using System.Numerics;

namespace HastlayerTimingTester
{
    /// <summary>
    /// Provides configuration for Hastlayer Timing Tester.
    /// You need to edit the source and recompile the application to change configuration.
    /// See the comments in the source of this class for detailed description of the options.
    /// It is advised to check <see cref="Operators"> first.
    /// </summary>
    internal class TimingTestConfig : TimingTestConfigBase
    {
        public TimingTestConfig()
        {
            // There are lists of functions below that can generate input data types to test.
            // Any of these can be used as a parameter to the constructor of VhdlOp.
            // (It is advised to start with the Operators variable when looking at this file.)
            // For example, for an input size of 32, we should get "unsigned(31 downto 0)" to be pasted into the VHDL
            // template. However, if a friendly name is requested instead, "unsigned32" is returned, which can be safely
            // used in directory names.
            var suNumericDataTypes = new List<VhdlOp.DataTypeFromSizeDelegate> {
                (size, getFriendlyName) =>
                    (getFriendlyName) ?
                        string.Format("unsigned{0}", size) :
                        string.Format("unsigned({0} downto 0)", size-1),
                (size, getFriendlyName) =>
                    (getFriendlyName) ?
                        string.Format("signed{0}", size) :
                        string.Format("signed({0} downto 0)", size-1)
            };
            var stdLogicVectorDataType = new List<VhdlOp.DataTypeFromSizeDelegate> {
                (size, getFriendlyName) =>
                    (getFriendlyName) ?
                        string.Format("std_logic_vector{0}", size) :
                        string.Format("std_logic_vector({0} downto 0)", size-1)
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
                new VhdlOp(new UnaryOperatorVhdlExpression("+", UnaryOperatorVhdlExpression.ValidationMode.AnyDataType),
                                                                      "unary_plus",  suNumericDataTypes, VhdlOp.SameOutputDataType,       defaultVhdlTemplates),
                new VhdlOp(new UnaryOperatorVhdlExpression("-", UnaryOperatorVhdlExpression.ValidationMode.SignedOnly),
                                                                      "unary_minus",  suNumericDataTypes, VhdlOp.SameOutputDataType,       defaultVhdlTemplates),
                new VhdlOp(new WrapSmartResizeVhdlExpression(new BinaryOperatorVhdlExpression("/")),
                                                                      "div",  suNumericDataTypes,     VhdlOp.SameOutputDataType,        defaultVhdlTemplates),
                new VhdlOp(new WrapSmartResizeVhdlExpression(new BinaryOperatorVhdlExpression("*")),
                                                                      "mul",  suNumericDataTypes,     VhdlOp.SameOutputDataType,         defaultVhdlTemplates),
            };

            // We test shifting by the amount of bits listed below. 
            // Multiplying by a constant 2^N is also a shift operation, so 
            // we test that here, too. As we expect the FPGA compiler to implement this by wiring, multiplying by 2^N is
            // expected to be faster than multiplying by another constant or another variable (where it would use DSP
            // blocks).
            for (int i = 0; i < 64; i++) // <-- bit shift amounts to test
            {
                // These are the original shift test cases, however the DotnetShiftVhdlExpression implements the
                // expression that Hastlayer really uses:
                // Operators.Add(new VhdlOp(new ShiftVhdlExpression(ShiftVhdlExpression.Direction.Left, i),
                //     "shift_left_by_" + i.ToString(), suNumericDataTypes, VhdlOp.SameOutputDataType, defaultVhdlTemplates));
                // Operators.Add(new VhdlOp(new ShiftVhdlExpression(ShiftVhdlExpression.Direction.Right, i),
                //     "shift_right_by_" + i.ToString(), suNumericDataTypes, VhdlOp.SameOutputDataType, defaultVhdlTemplates));

                Operators.Add(new VhdlOp(new DotnetShiftVhdlExpression(DotnetShiftVhdlExpression.Direction.Left, 64, true, false, i),
                    "dotnet_shift_left_by_" + i.ToString(), suNumericDataTypes, VhdlOp.SameOutputDataType, defaultVhdlTemplates));
                Operators.Add(new VhdlOp(new DotnetShiftVhdlExpression(DotnetShiftVhdlExpression.Direction.Right, 64, true, false, i),
                    "dotnet_shift_right_by_" + i.ToString(), suNumericDataTypes, VhdlOp.SameOutputDataType, defaultVhdlTemplates));
                Operators.Add(new VhdlOp(new DotnetShiftVhdlExpression(DotnetShiftVhdlExpression.Direction.Left, 32, true, false, i),
                    "dotnet_shift_left_by_" + i.ToString(), suNumericDataTypes, VhdlOp.SameOutputDataType, defaultVhdlTemplates));
                Operators.Add(new VhdlOp(new DotnetShiftVhdlExpression(DotnetShiftVhdlExpression.Direction.Right, 32, true, false, i),
                    "dotnet_shift_right_by_" + i.ToString(), suNumericDataTypes, VhdlOp.SameOutputDataType, defaultVhdlTemplates));

                // These are test cases for /(2^n) or *(2^n) which is practically just a shift:
                var powTwoOfI = BigInteger.Pow(2, i);
                Operators.Add(new VhdlOp(new MutiplyDivideByConstantVhdlExpression(powTwoOfI,
                     MutiplyDivideByConstantVhdlExpression.Mode.Multiply,
                     MutiplyDivideByConstantVhdlExpression.ValidationMode.UnsignedOnly),
                     "mul_by_" + powTwoOfI.ToString(), suNumericDataTypes, VhdlOp.SameOutputDataType, defaultVhdlTemplates));
                Operators.Add(new VhdlOp(new MutiplyDivideByConstantVhdlExpression(powTwoOfI,
                     MutiplyDivideByConstantVhdlExpression.Mode.Multiply,
                     MutiplyDivideByConstantVhdlExpression.ValidationMode.SignedOnly),
                     "mul_by_" + powTwoOfI.ToString(), suNumericDataTypes, VhdlOp.SameOutputDataType, defaultVhdlTemplates));
                Operators.Add(new VhdlOp(new MutiplyDivideByConstantVhdlExpression(powTwoOfI,
                    MutiplyDivideByConstantVhdlExpression.Mode.Divide,
                    MutiplyDivideByConstantVhdlExpression.ValidationMode.UnsignedOnly),
                     "div_by_" + powTwoOfI.ToString(), suNumericDataTypes, VhdlOp.SameOutputDataType, defaultVhdlTemplates));
                Operators.Add(new VhdlOp(new MutiplyDivideByConstantVhdlExpression(powTwoOfI,
                    MutiplyDivideByConstantVhdlExpression.Mode.Divide,
                    MutiplyDivideByConstantVhdlExpression.ValidationMode.SignedOnly),
                     "div_by_" + powTwoOfI.ToString(), suNumericDataTypes, VhdlOp.SameOutputDataType, defaultVhdlTemplates));
            }

            // These are DotnetShiftExpression with a<<b where both are variables:
            foreach (int outputSize in new List<int> { 32, 64 })
            {
                Operators.Add(new VhdlOp(
                        new DotnetShiftVhdlExpression(DotnetShiftVhdlExpression.Direction.Left, outputSize, false, false),
                        "dotnet_shift_left", suNumericDataTypes, VhdlOp.SameOutputDataType, defaultVhdlTemplates
                    ));
                Operators.Add(new VhdlOp(
                        new DotnetShiftVhdlExpression(DotnetShiftVhdlExpression.Direction.Right, outputSize, false, false),
                        "dotnet_shift_right", suNumericDataTypes, VhdlOp.SameOutputDataType, defaultVhdlTemplates
                    ));
            }

            // Just to test MultiplyDivideByConstantVhdlExpression behaviour on constant 0 or negative numbers:
            Operators.Add(new VhdlOp(new MutiplyDivideByConstantVhdlExpression(-2,
                 MutiplyDivideByConstantVhdlExpression.Mode.Multiply,
                 MutiplyDivideByConstantVhdlExpression.ValidationMode.SignedOnly),
                 "mul_by_-2", suNumericDataTypes, VhdlOp.SameOutputDataType, defaultVhdlTemplates));
            Operators.Add(new VhdlOp(new MutiplyDivideByConstantVhdlExpression(0,
                 MutiplyDivideByConstantVhdlExpression.Mode.Multiply,
                 MutiplyDivideByConstantVhdlExpression.ValidationMode.UnsignedOnly),
                 "mul_by_0", suNumericDataTypes, VhdlOp.SameOutputDataType, defaultVhdlTemplates));


            // InputSizes is the list of input sizes for the data type that we want to test
            InputSizes = new List<int> { 1, 8, 16, 32, 64 };

            Part = "xc7a100tcsg324-1"; // The FPGA part number (only used for Xilinx devices)


            Frequency = 100e6m; // System clock frequency in Hz
            //Frequency = 150e6m; // System clock frequency in Hz
            Name = "default"; // Name of the configuration, will be used in the name of the output directory

            // If DebugMode is true, the Hastlayer Timing Tester will stop at any exceptions during tests.
            // If it is false, the exceptions are logged and the program continues with the next test.
            DebugMode = false;

            // This selects for which FPGA vendor do we want to run the timing test. 
            // XilinxDriver supports Vivado.
            Driver = new XilinxDriver(this, @"C:\Xilinx\Vivado\2016.4\bin\vivado.bat");

            // IntelDriver supports Quartus and TimeQuest.
            //Driver = new IntelDriver(this, @"C:\altera\15.1\quartus\bin64");

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

            // It sets the number of threads used during simulation, if the FPGA vendor tools supports it. 
            // (Currently only Vivado supports multi threading.)
            NumberOfThreads = 8;
        }
    }
}
