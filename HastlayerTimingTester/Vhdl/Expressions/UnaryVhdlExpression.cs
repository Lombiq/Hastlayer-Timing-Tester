using System;
using System.Collections.Generic;

namespace HastlayerTimingTester.Vhdl.Expressions
{
    /// <summary>
    /// Generates a VHDL expression for a unary operator (e.g. not a).
    /// </summary>
    internal class UnaryOperatorVhdlExpression : VhdlExpressionBase
    {
        private readonly string _vhdlOperator;
        private readonly ValidationMode _validationMode;

        public enum ValidationMode
        {
            SignedOnly,
            AnyDataType,
        }

        /// <param name="vhdlOperator">The operator symbol/string (e.g. "not").</param>
        public UnaryOperatorVhdlExpression(string vhdlOperator, ValidationMode validationMode = ValidationMode.AnyDataType)
        {
            _vhdlOperator = vhdlOperator;
            _validationMode = validationMode;
        }

        /// <summary>
        /// Returns VHDL code for the binary operator.
        /// </summary>
        /// <param name="inputs">The single operand for the operator.</param>
        /// <param name="inputSize">The number of bits per input.</param>
        public override string GetVhdlCode(IReadOnlyList<string> inputs, int inputSize) => _vhdlOperator + " " + inputs[0];

        /// <summary>
        /// See <see cref="VhdlExpressionBase.IsValid"/>. Here we don't want to make any restriction on valid test
        /// cases, as this class might generate test cases for a wide range of operators. Note that because of this a
        /// lot of cases will fail with something like "0 definitions of operator "+" match here".
        /// </summary>
        public override bool IsValid(
            int inputSize,
            VhdlOp.DataTypeFromSizeDelegate inputDataTypeFunction,
            VhdlTemplateBase vhdlTemplate) =>
            _validationMode != ValidationMode.SignedOnly || inputDataTypeFunction(0, true).StartsWith("signed", StringComparison.InvariantCulture);
    }
}
