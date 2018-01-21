using System;

namespace HastlayerTimingTester
{
    public class MutiplyDivideByConstantVhdlExpression : VhdlExpressionBase
    {
        public enum Mode
        {
            Multiply, Divide
        }
        private double _constant;
        private Mode _mode;

        public MutiplyDivideByConstantVhdlExpression(double constant, Mode mode)
        {
            _constant = constant;
            _mode = mode;
        }

        public override string GetVhdlCode(string[] inputs) =>
            string.Format("{0} {1} {2}", inputs[0], (_mode==Mode.Multiply)?"*":"/", _constant.ToString("0"));

        public override bool IsValid(int inputSize, VhdlOp.DataTypeFromSizeDelegate inputDataTypeFunction,
            VhdlTemplateBase vhdlTemplate)
        { return Math.Log(_constant, 2) + 1 <= inputSize; }

    }
}
