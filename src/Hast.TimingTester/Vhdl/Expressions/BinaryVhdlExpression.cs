using System.Collections.Generic;

namespace HastlayerTimingTester.Vhdl.Expressions;

/// <summary>
/// Generates a VHDL expression for a binary operator (e.g. a+b, a-b, a*b, etc.)
/// </summary>
public class BinaryOperatorVhdlExpression : VhdlExpressionBase
{
    private readonly string _vhdlOperator;

    /// <param name="vhdlOperator">The operator symbol/string (e.g. "+", "-", "*", etc.)</param>
    public BinaryOperatorVhdlExpression(string vhdlOperator) => _vhdlOperator = vhdlOperator;

    /// <summary>
    /// Returns VHDL code for the binary operator.
    /// </summary>
    /// <param name="inputs">The two operands for the operator.</param>
    /// <param name="inputSize">The number of bits per input.</param>
    public override string GetVhdlCode(IReadOnlyList<string> inputs, int inputSize) => $"{inputs[0]} {_vhdlOperator} {inputs[1]}";

    /// <summary>
    /// See <see cref="VhdlExpressionBase.IsValid"/>. Here we don't want to make any restriction on valid test cases, as
    /// this class might generate test cases for a wide range of operators.
    /// </summary>
    public override bool IsValid(
        int inputSize,
        VhdlOp.DataTypeFromSizeDelegate
        inputDataTypeFunction,
        VhdlTemplateBase vhdlTemplate) => true;
}
