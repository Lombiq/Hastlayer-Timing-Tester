using System;

namespace HastlayerTimingTester
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
    /// </summary>
    public class DotnetShiftVhdlExpression : VhdlExpressionBase
    {
        public enum Direction
        {
            Left, Right
        }
        private int _amount;
        private Direction _direction;
        private int _outputSize;
        private bool _constantAmount;
        private bool _enableOnlyUnsigned;
        public const int NoOutputSizeCheck = -1;

        /// <summary>
        /// Used in <see cref="ShiftBitsMask"/>.
        /// </summary>
        private int ShiftBits(int size) => (int)Math.Log(size, 2);

        /// <summary>
        /// Used to generate a part of the expression in <see cref="GetVhdlCode"/>. 
        /// It will generate a series of "1" in a string, the amount of which is log2(output bits).
        /// It helps to achieve the dotnet way of shifting 
        /// (e.g. shifting a 32-bit number by 33 will result in an 1-shift, see <see cref="DotnetShiftVhdlExpression"/>
        /// class definition docstring summary).
        /// </summary>
        private string ShiftBitsMask(int size)
        {
            string mask = "";
            for (int i = 0; i < ShiftBits(size); i++)
            {
                mask += "1";
            }
            return mask;
        }

        /// <param name="direction">is the direction of the shift (left or right).</param>
        /// <param name="amount">is the number of bits to shift.</param>
        public DotnetShiftVhdlExpression(Direction direction, int outputSize, bool constantAmount,
            bool enableOnlyUnsigned = false, int amount = 0)
        {
            _direction = direction;
            _amount = amount;
            _constantAmount = constantAmount;
            _outputSize = outputSize;
            _enableOnlyUnsigned = enableOnlyUnsigned;
        }

        /// <param name="inputs">is the input to the shift.</param>
        /// <param name="inputSize">is the input size in bits.</param>
        /// <returns>the VHDL code.</returns>
        public override string GetVhdlCode(string[] inputs, int inputSize)
        {
            // Real-life example from KPZ Hast_IP:
            // shift_right(num4, to_integer(unsigned(SmartResize(to_signed(16, 32), 5) and "11111")));
            int size = (_outputSize == NoOutputSizeCheck) ? inputSize : _outputSize;
            return (!_constantAmount) ?
                string.Format(
                    "shift_{0}({1},  to_integer(unsigned(SmartResize({2}, {3}) and \"{4}\"))  )",
                    (_direction == Direction.Left) ? "left" : "right",   // {0}
                    inputs[0],                                           // {1}
                    inputs[1],                                           // {2}
                    ShiftBits(size),                                     // {3}
                    ShiftBitsMask(size)                                  // {4}
                    ) :
                string.Format("shift_{0}({1},  to_integer(unsigned(SmartResize(to_signed({2}, {3}), {4}) and \"{5}\"))  )",
                   (_direction == Direction.Left) ? "left" : "right",  // {0}
                   inputs[0],                                          // {1}
                   _amount,                                            // {2}
                   size,                                               // {3}
                   ShiftBits(size),                                    // {4}
                   ShiftBitsMask(size)                                 // {5}
                   );
        }

        /// <summary>
        /// See <see cref="VhdlExpressionBase.IsValid"/>. Testing a shifting with an equal or greater amount of bits 
        /// than the input size makes no sense, so we impose a restriction on this. 
        /// We only work on test cases where input size and output size is the same, because of the complicated
        /// expression (including SmartResizes) in <see cref="GetVhdlCode"/>. 
        /// </summary>
        public override bool IsValid(int inputSize, VhdlOp.DataTypeFromSizeDelegate inputDataTypeFunction,
            VhdlTemplateBase vhdlTemplate)
        {
            return (_outputSize == NoOutputSizeCheck) ? true : inputSize == _outputSize &&
                (_constantAmount) ? inputSize > _amount : true &&
                (_enableOnlyUnsigned) ? inputDataTypeFunction(0, true).StartsWith("unsigned") : true;
        }

    }
}
