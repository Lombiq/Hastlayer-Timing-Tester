using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;

namespace HastlayerTimingTester.Vhdl.Expressions;

/// <summary>
/// Generates a VHDL expression for multiplying an input signal with a constant number. It uses SmartResize. It only
/// works for unsigned test cases.
/// </summary>
public class MutiplyDivideByConstantVhdlExpression : VhdlExpressionBase
{
    public enum Mode
    {
        Multiply,
        Divide,
    }

    public enum ValidationMode
    {
        SignedOnly,
        UnsignedOnly,
    }

    private readonly Mode _mode;
    private readonly ValidationMode _validationMode;
    private readonly BigInteger _constant;

    /// <param name="constant">The constant to multiply/divide with.</param>
    /// <param name="mode">It selects whether we want to multiply or divide.</param>
    public MutiplyDivideByConstantVhdlExpression(BigInteger constant, Mode mode, ValidationMode validationMode)
    {
        _constant = constant;
        _mode = mode;
        _validationMode = validationMode;
    }

    /// <param name="inputs">The input to the shift.</param>
    /// <param name="inputSize">The input size in bits.</param>
    /// <returns>The VHDL code.</returns>
    public override string GetVhdlCode(IReadOnlyList<string> inputs, int inputSize) =>
        string.Format(
            CultureInfo.InvariantCulture,
            "SmartResize({0} {1} {4}signed'(x\"{2}\"), {3})",
            inputs[0],
            (_mode == Mode.Multiply) ? "*" : "/",
            (_constant == 0) ? "0" : _constant.ToString("x" + (inputSize / 4).ToString(CultureInfo.InvariantCulture), CultureInfo.InvariantCulture),
            inputSize,
            (_validationMode == ValidationMode.UnsignedOnly) ? "un" : string.Empty
            );

    /// <summary>
    /// This VHDL expression only makes sense if the constant fits into less bits than the input size, as if it did not,
    /// the result would be 0 anyway.
    /// </summary>
    public override bool IsValid(
        int inputSize,
        VhdlOp.DataTypeFromSizeDelegate inputDataTypeFunction,
        VhdlTemplateBase vhdlTemplate)
    {
        var signedMode = inputDataTypeFunction(0, getFriendlyName: true).StartsWith("signed", StringComparison.InvariantCulture);
        var unsignedMode = inputDataTypeFunction(0, getFriendlyName: true).StartsWith("unsigned", StringComparison.InvariantCulture);
        return (Math.Log((double)_constant, 2) + 1 + (signedMode ? 1 : 0) <= inputSize || _constant < 0) &&
                (_validationMode != ValidationMode.UnsignedOnly || unsignedMode) &&
                (_validationMode != ValidationMode.SignedOnly || signedMode);
    }
}
