using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace hastlayer_timing_tester
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
        }

        override public void processResults(vivadoResult result)
        {
            string dataPathDelayLine = "";
            Regex.Split(result.timingReport, "\r\n").ToList().ForEach((x) => { if (x.Contains("Data Path Delay")) dataPathDelayLine = x; });
            string tempLinePart = Regex.Split(dataPathDelayLine, "\\(logic")[0];
            tempLinePart = Regex.Split(tempLinePart, "Data Path Delay:")[1];
            tempLinePart = Regex.Split(tempLinePart, "ns")[0];
            tempLinePart = tempLinePart.Trim();
            float dataPathDelay = float.Parse(tempLinePart, CultureInfo.InvariantCulture);
            Console.WriteLine("Data path delay = {0} ns;  Max clock frequency = {1} MHz", dataPathDelay, Math.Floor((1 / (dataPathDelay * 1e-9)) / 1000) / 1000);
        }
    }
}
