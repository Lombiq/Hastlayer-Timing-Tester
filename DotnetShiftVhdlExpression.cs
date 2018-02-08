using System;

namespace HastlayerTimingTester
{
    /// <summary>
    /// Generates a VHDL expression for shift_left or shift_right.
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

        private int ShiftBits => (int)Math.Log(_outputSize, 2);
        private string ShiftBitsMask {
            get
            {
                string mask = "";
                for(int i=0; i<ShiftBits; i++)
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
            //Real-life example from KPZ Hast_IP:
            //shift_right(num4, to_integer(unsigned(SmartResize(to_signed(16, 32), 5) and "11111")));
            string.Format("shift_{0}({1},  to_integer(unsigned(SmartResize(to_signed({2}, {3}), {4}) and \"{5}\"))  )",
                (_direction == Direction.Left) ? "left" : "right",  //{0}
                inputs[0],    //{1}
                _amount,      //{2}
                _outputSize,  //{3}
                ShiftBits,    //{4}
                ShiftBitsMask //{5}
                );

        /// <summary>
        /// See <see cref="VhdlExpressionBase.IsValid"/>. Testing a shifting with an equal or greater amount of bits 
        /// than the input size makes no sense, so we impose a restriction on this. 
        /// </summary>
        public override bool IsValid(int inputSize, DataTypes.Base inputDataType, 
            VhdlTemplateBase vhdlTemplate)
        { return inputSize == _outputSize && inputSize > _amount; }
    }
}
