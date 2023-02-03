using System.Collections.Generic;
using System.Globalization;

namespace HastlayerTimingTester.Vhdl.Expressions;

/// <summary>
/// Wraps another VHDL expression in SmartResize(), which is defined in <see cref="AdditionalVhdlIncludes"/>. The size
/// parameter of SmartResize will be set to the input size. For example, those operators that normally required <see
/// cref="VhdlOp.DoubleSizedOutput"/> can now work with <see cref="VhdlOp.SameOutputDataType"/> using
/// WeapSmartResizeVhdlExpression.
/// </summary>
public class WrapSmartResizeVhdlExpression : VhdlExpressionBase
{
    private readonly VhdlExpressionBase _innerExpression;

    public WrapSmartResizeVhdlExpression(VhdlExpressionBase innerExpression) => _innerExpression = innerExpression;

    public override string GetVhdlCode(IReadOnlyList<string> inputs, int inputSize) =>
        "SmartResize(" + _innerExpression.GetVhdlCode(inputs, inputSize) + ", " + inputSize.ToString(CultureInfo.InvariantCulture) + ")";

    /// <summary>
    /// See <see cref="VhdlExpressionBase.IsValid"/>. Here we'll make the same restrictions as the wrapped
    /// expression.
    /// </summary>
    public override bool IsValid(int inputSize, VhdlOp.DataTypeFromSizeDelegate inputDataTypeFunction, VhdlTemplateBase vhdlTemplate) =>
        _innerExpression.IsValid(inputSize, inputDataTypeFunction, vhdlTemplate);
}
