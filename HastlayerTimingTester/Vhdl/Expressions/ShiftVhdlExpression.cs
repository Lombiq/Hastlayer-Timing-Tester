using System.Collections.Generic;

namespace HastlayerTimingTester.Vhdl.Expressions;

/// <summary>
/// Generates a VHDL expression for shift_left or shift_right.
/// </summary>
public class ShiftVhdlExpression : VhdlExpressionBase
{
    public enum Direction
    {
        Left,
        Right,
    }

    private readonly int _amount;
    private readonly Direction _direction;

    /// <param name="direction">The direction of the shift (left or right).</param>
    /// <param name="amount">The number of bits to shift.</param>
    public ShiftVhdlExpression(Direction direction, int amount)
    {
        _direction = direction;
        _amount = amount;
    }

    /// <summary>
    /// It returns the VHDL code.
    /// </summary>
    /// <param name="inputs">The input to the shift.</param>
    public override string GetVhdlCode(IReadOnlyList<string> inputs, int inputSize) =>
        $"shift_{(_direction == Direction.Left ? "left" : "right")}({inputs[0]},{_amount})";

    /// <summary>
    /// See <see cref="VhdlExpressionBase.IsValid"/>. Testing a shifting with an equal or greater amount of bits than
    /// the input size makes no sense, so we impose a restriction on this.
    /// </summary>
    public override bool IsValid(
        int inputSize,
        VhdlOp.DataTypeFromSizeDelegate inputDataTypeFunction,
        VhdlTemplateBase vhdlTemplate) => inputSize > _amount;
}
