using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HastlayerTimingTester
{
    public class BinaryOperatorVhdlExpression : VhdlExpressionBase
    {
        string vhdlOperator;

        public BinaryOperatorVhdlExpression(string vhdlOperator)
        {
            this.vhdlOperator = vhdlOperator;
        }

        public override string GetVhdlCode(string[] inputs)
        {
            return String.Format("{0} {1} {2}", inputs[0], vhdlOperator, inputs[1]);
        }
    }
}
