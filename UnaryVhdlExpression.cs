﻿namespace HastlayerTimingTester
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
        public override string GetVhdlCode(string[] inputs, int inputSize) => string.Format("{0} {1}", _vhdlOperator, inputs[0]);

        /// <summary>
        /// See <see cref="VhdlExpressionBase.IsValid"/>. Here we don't want to make any restriction on valid test 
        /// cases, as this class might generate test cases for a wide range of operators.
        /// </summary>
        public override bool IsValid(int inputSize, DataTypes.Base inputDataType, 
            VhdlTemplateBase vhdlTemplate)
        { return true; }
    }
}
