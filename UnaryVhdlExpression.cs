namespace HastlayerTimingTester
{    
    /// <summary>
    /// Generates a VHDL expression for a unary operator (e.g. not a)
    /// </summary>
    class UnaryOperatorVhdlExpression : VhdlExpressionBase
    {
        private string _vhdlOperator;

        /// <param name="vhdlOperator">is the operator symbol/string (e.g. "not")</param>
        public UnaryOperatorVhdlExpression(string vhdlOperator)
        {
            _vhdlOperator = vhdlOperator;
        }

        /// <summary>
        /// Returns VHDL code for the binary operator.
        /// </summary>
        /// <param name="inputs">is the single operand for the operator.</param>
        public override string GetVhdlCode(string[] inputs) => string.Format("{0} {1}", _vhdlOperator, inputs[0]);
    }
}
