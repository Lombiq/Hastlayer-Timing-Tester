namespace HastlayerTimingTester
{
    public class BinaryOperatorVhdlExpression : VhdlExpressionBase
    {
        private string _vhdlOperator;

        public BinaryOperatorVhdlExpression(string vhdlOperator)
        {
            this._vhdlOperator = vhdlOperator;
        }

        public override string GetVhdlCode(string[] inputs) =>
            string.Format("{0} {1} {2}", inputs[0], _vhdlOperator, inputs[1]);
    }
}
