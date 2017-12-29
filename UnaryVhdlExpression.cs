namespace HastlayerTimingTester
{    
    /// <summary>
    /// Generates a VHDL expression for a unary operator (e.g. not a)
    /// </summary>
    class UnaryOperatorVhdlExpression : VhdlExpressionBase
    {
        string vhdlOperator;

        /// <param name="vhdlOperator">is the operator symbol/string (e.g. "not")</param>
        public UnaryOperatorVhdlExpression(string vhdlOperator)
        {
            this.vhdlOperator = vhdlOperator;
        }

        /// <summary>
        /// It returns VHDL code for the binary operator.
        /// </summary>
        /// <param name="inputs">is the single operand for the operator.</param>
        public override string GetVhdlCode(string[] inputs) => string.Format("{0} {1}", vhdlOperator, inputs[0]);
    }
}
