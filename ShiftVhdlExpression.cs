namespace HastlayerTimingTester
{
    /// <summary>
    /// Generates a VHDL expression for shift_left or shift_right.
    /// </summary>
    public class ShiftVhdlExpression : VhdlExpressionBase
    {
        public enum Direction
        {
            Left, Right
        }
        private int _amount;
        private Direction _direction;

        /// <param name="direction">is the direction of the shift (left or right).</param>
        /// <param name="amount">is the number of bits to shift.</param>
        public ShiftVhdlExpression(Direction direction, int amount)
        {
            _direction = direction;
            _amount = amount;
        }

        /// <summary>
        /// It returns the VHDL code.
        /// </summary>
        /// <param name="inputs">is the input to the shift.</param>
        public override string GetVhdlCode(string[] inputs) =>
            string.Format("shift_{0}({1},{2})",
                (_direction == Direction.Left) ? "left" : "right", inputs[0], _amount);

        /// <summary>
        /// See <see cref="VhdlExpressionBase.IsValid"/>. Testing a shifting with an equal or greater amount of bits 
        /// than the input size makes no sense, so we impose a restriction on this. 
        /// </summary>
        public override bool IsValid(int inputSize, VhdlOp.DataTypeFromSizeDelegate inputDataTypeFunction, 
            VhdlTemplateBase vhdlTemplate)
        { return inputSize > _amount; }
    }
}
