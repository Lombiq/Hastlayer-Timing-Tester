using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace HastlayerTimingTester
{

    class TimingOutputParser
    {

        private float _DataPathDelay;
        private bool _DataPathDelayAvailable = false;

        public float DataPathDelay { get { if(!_DataPathDelayAvailable) throw new Exception("data path delay is not available"); return _DataPathDelay; }}
        public bool DataPathDelayAvailable { get { return _DataPathDelayAvailable; }}

        public void Parse(VivadoResult result)
        {
            Match myMatch = Regex.Match(result.TimingReport, @"(\s*)Data Path Delay:(\s*)([0-9\.]*)ns");
            if(myMatch.Success)
            {
                _DataPathDelay = float.Parse(myMatch.Groups[3].Value, CultureInfo.InvariantCulture);
                _DataPathDelayAvailable = true;
            }
        }
        public void PrintParsedTimingReport()
        {
            if(_DataPathDelayAvailable) Console.WriteLine("Data path delay = {0} ns;  Max clock frequency = {1} MHz", DataPathDelay, Math.Floor((1 / (DataPathDelay * 1e-9)) / 1000) / 1000);
        }
    }

}
