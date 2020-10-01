using System;
using System.Collections.Generic;
using System.Linq;

namespace HastlayerTimingTester.Vhdl.Expressions
{
    /// <summary>
    /// Generates a VHDL expression for shifting left or right, but the expression it generates conforms the
    /// specification of .NET shift operators. See this note from Hastlayer:
    /// "Contrary to what happens in VHDL binary shifting in .NET will only use the lower 5 bits (for 32b
    /// operands) or 6 bits (for 64b operands) of the shift count. So e.g. 1 &lt;&lt; 33 won't produce 0 (by
    /// shifting out to the void) but 2, since only a shift by 1 happens (as 33 is 100001 in binary).
    /// See: https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/left-shift-operator
    /// So we need to truncate.
    /// Furthermore right shifts will also do a bitwise AND with just 1s on the count, see:
    /// https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/right-shift-operator"
    /// It has two modes of operation: shift by a constant or a variable number or bits, which can be set in the
    /// constructor.
    /// </summary>
    public class DotnetShiftVhdlExpression : VhdlExpressionBase
    {
        public enum Direction
        {
            Left,
            Right,
        }

        public const int NoOutputSizeCheck = -1;

        private readonly int _amount;
        private readonly Direction _direction;
        private readonly int _outputSize;
        private readonly bool _constantAmount;
        private readonly bool _enableOnlyUnsigned;


        /// <param name="direction">The direction of the shift (left or right).</param>
        /// <param name="amount">The number of bits to shift.</param>
        /// <param name="constantAmount">Sets if the amount of bits to shift is constant or is also a variable.</param>
        /// <param name="enableOnlyUnsigned">Will ignore any signed test cases, if enabled.</param>
        /// <param name="outputSize">
        ///     Will ignore any test cases where the number of output bits does not equal this
        ///     parameter. This filter can be switched off by setting this parameter to <see cref="NoOutputSizeCheck"/>.
        /// </param>
        public DotnetShiftVhdlExpression(
            Direction direction,
            int outputSize,
            bool constantAmount,
            bool enableOnlyUnsigned = false,
            int amount = 0)
        {
            _direction = direction;
            _amount = amount;
            _constantAmount = constantAmount;
            _outputSize = outputSize;
            _enableOnlyUnsigned = enableOnlyUnsigned;
        }

        /// <param name="inputs">
        ///  The inputs to the shift. If <see cref="_constantAmount"/> was set to true, only the first input is used.
        /// </param>
        /// <param name="inputSize">The input size in bits.</param>
        /// <returns>The VHDL code.</returns>
        public override string GetVhdlCode(IReadOnlyList<string> inputs, int inputSize)
        {
            // Real-life example from KPZ Hast_IP:
            //// shift_right(num4, to_integer(unsigned(SmartResize(to_signed(16, 32), 5) and "11111")));
            int size = _outputSize == NoOutputSizeCheck ? inputSize : _outputSize;
            var direction = _direction == Direction.Left ? "left" : "right";
            return !_constantAmount
                ? $"shift_{direction}({inputs[0]},  to_integer(unsigned(SmartResize({inputs[1]}, {ShiftBits(size)}) and \"{ShiftBitsMask(size)}\"))  )"
                : $"shift_{direction}({inputs[0]},  to_integer(unsigned(SmartResize(to_signed({_amount}, {size}), {ShiftBits(size)}) and \"{ShiftBitsMask(size)}\"))  )";
        }

        /// <summary>
        /// See <see cref="VhdlExpressionBase.IsValid"/>. Testing a shifting with an equal or greater amount of bits
        /// than the input size makes no sense, so we impose a restriction on this.
        /// We only work on test cases where input size and output size is the same, because of the complicated
        /// expression (including SmartResizes) in <see cref="GetVhdlCode"/>.
        /// This check can be however switched off, see <see cref="_outputSize"/> in the constructor.
        /// </summary>
        public override bool IsValid(
            int inputSize,
            VhdlOp.DataTypeFromSizeDelegate inputDataTypeFunction,
            VhdlTemplateBase vhdlTemplate) =>
                // We're smart enough for this.
#pragma warning disable S1067 // Expressions should not be too complex
                (_outputSize == NoOutputSizeCheck || inputSize == _outputSize) &&
                (!_constantAmount || inputSize > _amount) &&
                (!_enableOnlyUnsigned || inputDataTypeFunction(0, true).StartsWith("unsigned", StringComparison.InvariantCulture));
#pragma warning restore S1067 // Expressions should not be too complex


        /// <summary>
        /// Used in <see cref="ShiftBitsMask"/>.
        /// </summary>
        private static int ShiftBits(int size) => (int)Math.Log(size, 2);

        /// <summary>
        /// Used to generate a part of the expression in <see cref="GetVhdlCode"/>.
        /// It will generate a series of "1" in a string, the amount of which is log2(number of bits).
        /// It helps to achieve the dotnet way of shifting
        /// (e.g. shifting a 32-bit number by 33 will result in an 1-shift, see <see cref="DotnetShiftVhdlExpression"/>
        /// class definition docstring summary).
        /// </summary>
        private static string ShiftBitsMask(int size) => string.Join(string.Empty, Enumerable.Repeat("1", ShiftBits(size)));
    }
}
