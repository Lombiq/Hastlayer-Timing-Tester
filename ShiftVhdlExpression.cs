using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HastlayerTimingTester
{
    public class ShiftVhdlExpression : VhdlExpressionBase
    {
        public enum Direction
        {
            Left, Right
        }

        private int _amount;
        private Direction _direction;

        public ShiftVhdlExpression(Direction direction, int amount)
        {
            _direction = direction;
            _amount = amount;
        }

        public override string GetVhdlCode(string[] inputs)
        {
            return String.Format("std_logic_vector(shift_{0}(unsigned({1}),{2}))", 
                (_direction == Direction.Left) ? "left" : "right", inputs[0], _amount);
        }
    }
}
