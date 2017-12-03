using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HastlayerTimingTester
{
    public abstract class VhdlExpressionBase
    {
        public abstract string GetVhdlCode(string[] inputs);
    }
}
