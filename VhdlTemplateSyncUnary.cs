using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;


namespace HastlayerTimingTester
{
    ///<summary>This is a VHDL template using an unary operator in a sequential, synchronous logic design.</summary>
    class VhdlTemplateSyncUnary : VhdlTemplateBase
    {
        public VhdlTemplateSyncUnary()
        {
            VhdlTemplate =
@"library ieee;
use ieee.std_logic_1164.all;
use ieee.numeric_std.all;

entity tf_sample is
port (
    clk     : in std_logic;
    a1      : in %INTYPE%;
    a2      : in %INTYPE%;
    aout    : out %OUTTYPE%
);
end tf_sample;

architecture imp of tf_sample is
    signal aout_reg : %OUTTYPE%;
    signal a1_reg : %INTYPE%;
    signal a2_reg : %INTYPE%;
begin
    clkpro : process(clk)
    begin
        if clk'event and clk = '1' then
            a1_reg <= a1;
            aout_reg <= %OPERATOR% a1_reg;
        end if;
    end process;
    aout <= aout_reg;
end imp;";
            XdcTemplate = "create_clock -period %CLKPERIOD% -name clk [get_ports {clk}]";
        }

        override public string Name { get { return "sync"; } }
    }
}
