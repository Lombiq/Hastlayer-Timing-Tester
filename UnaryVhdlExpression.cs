namespace HastlayerTimingTester
{
    /// <summary>
    /// Generates a VHDL expression for a unary operator (e.g. not a)
    /// </summary>
    class UnaryOperatorVhdlExpression : VhdlExpressionBase
    {
        private string _vhdlOperator;
        private ValidationMode _validationMode;

        public enum ValidationMode
        {
            SignedOnly, AnyDataType
        }


        /// <param name="vhdlOperator">is the operator symbol/string (e.g. "not")</param>
        public UnaryOperatorVhdlExpression(string vhdlOperator, ValidationMode validationMode = ValidationMode.AnyDataType)
        {
            _vhdlOperator = vhdlOperator;
            _validationMode = validationMode;
        }


        /// <summary>
        /// Returns VHDL code for the binary operator.
        /// </summary>
        /// <param name="inputs">is the single operand for the operator.</param>
        /// <param name="inputs">is the number of bits per input.</param>
        public override string GetVhdlCode(string[] inputs, int inputSize) => string.Format("{0} {1}", _vhdlOperator, inputs[0]);

        /// <summary>
        /// See <see cref="VhdlExpressionBase.IsValid"/>. Here we don't want to make any restriction on valid test 
        /// cases, as this class might generate test cases for a wide range of operators.
        /// </summary>
        public override bool IsValid(
            int inputSize,
            VhdlOp.DataTypeFromSizeDelegate inputDataTypeFunction,
            VhdlTemplateBase vhdlTemplate)
        {
            return true && ((_validationMode == ValidationMode.SignedOnly) ? inputDataTypeFunction(0, true).StartsWith("signed") : true);
        }
    }
}
