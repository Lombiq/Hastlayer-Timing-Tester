using System;

namespace HastlayerTimingTester
{
    /// <summary>
    /// Generates a VHDL expression for shifting left or right, but the expression it generates conforms 
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

        /// <summary>
        /// Used in <see cref="ShiftBitsMask"/>.
        /// </summary>
        private int ShiftBits => (int)Math.Log(_outputSize, 2);

        /// <summary>
        /// Used to generate a part of the expression in <see cref="GetVhdlCode"/>. 
        /// It will generate a series of "1" in a string, the amount of which is log2(output bits).
        /// It helps to achieve the dotnet way of shifting (e.g. shifting a 32-bit number by 33 will result in an 1-shift).  
        /// </summary>
        private string ShiftBitsMask
        {
            get
            {
                string mask = "";
                for (int i = 0; i < ShiftBits; i++)
                {
                    mask += "1";
                }
                return mask;
            }
        }

        /// <param name="direction">is the direction of the shift (left or right).</param>
        /// <param name="amount">is the number of bits to shift.</param>
        public DotnetShiftVhdlExpression(Direction direction, int amount, int outputSize)
        {
            _direction = direction;
            _amount = amount;
            _outputSize = outputSize;
        }


        /// <summary>
        /// It returns the VHDL code.
        /// </summary>
        /// <param name="inputs">is the input to the shift.</param>
        public override string GetVhdlCode(string[] inputs, int inputSize) =>
            // Real-life example from KPZ Hast_IP:
            // shift_right(num4, to_integer(unsigned(SmartResize(to_signed(16, 32), 5) and "11111")));
            string.Format("shift_{0}({1},  to_integer(unsigned(SmartResize(to_signed({2}, {3}), {4}) and \"{5}\"))  )",
                (_direction == Direction.Left) ? "left" : "right",  // {0}
                inputs[0],    // {1}
                _amount,      // {2}
                _outputSize,  // {3}
                ShiftBits,    // {4}
                ShiftBitsMask // {5}
                );

        /// <summary>
        /// See <see cref="VhdlExpressionBase.IsValid"/>. Testing a shifting with an equal or greater amount of bits 
        /// than the input size makes no sense, so we impose a restriction on this. 
        /// We only work on test cases where input size and output size is the same, because of the complicated
        /// expression (including SmartResizes) in <see cref="GetVhdlCode"/>. 
        /// </summary>
        public override bool IsValid(int inputSize, VhdlOp.DataTypeFromSizeDelegate inputDataTypeFunction,
            VhdlTemplateBase vhdlTemplate)
        { return inputSize == _outputSize && inputSize > _amount; }
    }
}
