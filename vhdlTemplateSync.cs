using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace hastlayer_timing_tester
{
    class vhdlTemplateSync : vhdlTemplateBase
    {

        public vhdlTemplateSync()
        {
            _template =
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
            a2_reg <= a2;
            aout_reg <= a1_reg %OPERATOR% a2_reg;
        end if;
    end process;
    aout <= aout_reg;
end imp;";
            _xdc = "create_clock -period 10.000 -name clk [get_ports {clk}]";
        }

        override public void processResults(vivadoResult result)
        {
            /*string dataPathDelayLine = "";
            Regex.Split(result.timingReport, "\r\n").ToList().ForEach((x) => { if (x.Contains("Data Path Delay")) dataPathDelayLine = x; });
            //string tempLinePart = Regex.Split(dataPathDelayLine, "\\(logic")[0];
            //tempLinePart = Regex.Split(tempLinePart, "Data Path Delay:")[1];
            //tempLinePart = Regex.Split(tempLinePart, "ns")[0];
            //tempLinePart = tempLinePart.Trim();
            if(dataPathDelayLine.Length == 0) { Console.WriteLine("Could not find \"Data Path Delay\" in timing report."); return; }
            Match myMatch = Regex.Match(result.timingReport, @"(\s*)Data Path Delay:(\s*)([0-9\.]*)ns");
            if(!myMatch.Success) { Console.WriteLine("Could not match regexp in timing report to find data path delay."); return; }
            float dataPathDelay = float.Parse(myMatch.Groups[3].Value, CultureInfo.InvariantCulture);
            Console.WriteLine("Data path delay = {0} ns;  Max clock frequency = {1} MHz", dataPathDelay, Math.Floor((1 / (dataPathDelay * 1e-9)) / 1000) / 1000);*/
        }

        override public string name { get { return "sync"; } }
    }
}
