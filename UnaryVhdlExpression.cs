namespace HastlayerTimingTester
{
    class UnaryOperatorVhdlExpression : VhdlExpressionBase
    {
        string vhdlOperator;

        public UnaryOperatorVhdlExpression(string vhdlOperator)
        {
            this.vhdlOperator = vhdlOperator;
        }

        public override string GetVhdlCode(string[] inputs) => string.Format("{0} {1}", vhdlOperator, inputs[0]);
    }
}
