using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HastlayerTimingTester
{
    class UnaryOperatorVhdlExpression : VhdlExpressionBase
    {
        string vhdlOperator;

        public UnaryOperatorVhdlExpression(string vhdlOperator)
        {
            this.vhdlOperator = vhdlOperator;
        }

        public override string GetVhdlCode(string[] inputs) => String.Format("{0} {1}", vhdlOperator, inputs[0]);
    }
}
