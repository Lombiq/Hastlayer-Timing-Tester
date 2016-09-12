using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace HastlayerTimingTester
{
    class vhdlTemplateAsync : vhdlTemplateBase
    {

        public vhdlTemplateAsync()
        {
            _template =
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
            _xdc = "";
        }

        override public string name { get { return "async"; } }
    }
}
