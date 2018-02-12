using System;
using System.Numerics;

namespace HastlayerTimingTester
{
    /// <summary>
    /// Generates a VHDL expression for multiplying an input signal with a constant number. It uses SmartResize.
    /// It only works for unsigned test cases. 
    /// </summary>
    public class MutiplyDivideByConstantVhdlExpression : VhdlExpressionBase
    {
        public enum Mode
        {
            Multiply, Divide
        }
        private BigInteger _constant;
        private Mode _mode;

        /// <param name="constant">is the constant to multiply/divide with.</param>
        /// <param name="mode">selects whether we want to multiply or divide.</param>
        public MutiplyDivideByConstantVhdlExpression(BigInteger constant, Mode mode)
        {
            _constant = constant;
            _mode = mode;
        }

        /// <summary>
        /// Cuts any "0" characters from the beginning of an input string, and returns the trimmed string.
        /// </summary>
        private static string CutZerosFromBeginning(string input)
        {
            while (input.Length > 0 && input.StartsWith("0")) input = input.Substring(1);
            return input;
        }

        /// <param name="inputs">is the input to the shift.</param>
        /// <param name="inputSize">is the input size in bits.</param>
        /// <returns>the VHDL code.</returns>
        public override string GetVhdlCode(string[] inputs, int inputSize) =>
            string.Format("SmartResize({0} {1} unsigned'(x\"{2}\"), {3})",
                inputs[0], (_mode == Mode.Multiply) ? "*" : "/", 
                CutZerosFromBeginning(_constant.ToString("x")), inputSize);

        /// <summary>
        /// This VHDL expression only makes sense if the constant fits into less bits than the input size
        /// </summary>
        public override bool IsValid(int inputSize, VhdlOp.DataTypeFromSizeDelegate inputDataTypeFunction,
            VhdlTemplateBase vhdlTemplate)
        {
            return Math.Log((double)_constant, 2) + 1 <= inputSize &&
                inputDataTypeFunction(0, true).StartsWith("unsigned");
        }

    }
}
