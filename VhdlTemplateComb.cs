using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

//This is a VHDL template using the operator in an combinatorial logic design.
//This means that there is no clock signal or flip-flops in the design, and the output is never fed back to the input.

namespace HastlayerTimingTester
{
    class VhdlTemplateComb : VhdlTemplateBase
    {

        public VhdlTemplateComb()
        {
            VhdlTemplate =
@"library ieee;
use ieee.std_logic_1164.all;
use ieee.numeric_std.all;

entity tf_sample is
port(
    a1      : in %INTYPE%;
    a2      : in %INTYPE%;
    aout    : out %OUTTYPE%
);
end tf_sample;

architecture imp of tf_sample is begin
    aout <= a1 %OPERATOR% a2;
end imp;";
            XdcTemplate = "";
        }

        override public string Name { get { return "comb"; } }
    }
}