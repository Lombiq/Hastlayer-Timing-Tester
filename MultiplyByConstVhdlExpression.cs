using System;
using System.Numerics;

namespace HastlayerTimingTester
{
    public class MutiplyDivideByConstantVhdlExpression : VhdlExpressionBase
    {
        public enum Mode
        {
            Multiply, Divide
        }
        private BigInteger _constant;
        private Mode _mode;

        public MutiplyDivideByConstantVhdlExpression(BigInteger constant, Mode mode)
        {
            _constant = constant;
            _mode = mode;
        }

        private static string CutZerosFromBeginning(string input)
        {
            while(input.Length > 0 && input.StartsWith("0")) input = input.Substring(1);
            return input;
        }

        public override string GetVhdlCode(string[] inputs, int inputSize) =>
            string.Format("SmartResize({0} {1} unsigned'(x\"{2}\"), {3})", 
                inputs[0], (_mode==Mode.Multiply)?"*":"/", CutZerosFromBeginning(_constant.ToString("x")), inputSize);

        public override bool IsValid(int inputSize, VhdlOp.DataTypeFromSizeDelegate inputDataTypeFunction,
            VhdlTemplateBase vhdlTemplate)
        { return Math.Log((double)_constant, 2) + 1 <= inputSize; }

    }
}
