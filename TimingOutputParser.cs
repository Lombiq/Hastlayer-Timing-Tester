using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace HastlayerTimingTester
{

    class TimingOutputParser
    {

        private float _dataPathDelay;
        private bool _dataPathDelayAvailable = false;

        public float dataPathDelay { get { if(!_dataPathDelayAvailable) throw new Exception("data path delay is not available"); return _dataPathDelay; }}
        public bool dataPathDelayAvailable { get { return _dataPathDelayAvailable; }}

        public void Parse(vivadoResult result)
        {
            Match myMatch = Regex.Match(result.timingReport, @"(\s*)Data Path Delay:(\s*)([0-9\.]*)ns");
            if(myMatch.Success)
            {
                _dataPathDelay = float.Parse(myMatch.Groups[3].Value, CultureInfo.InvariantCulture);
                _dataPathDelayAvailable = true;
            }
        }
        public void PrintParsedTimingReport()
        {
            if(_dataPathDelayAvailable) Console.WriteLine("Data path delay = {0} ns;  Max clock frequency = {1} MHz", dataPathDelay, Math.Floor((1 / (dataPathDelay * 1e-9)) / 1000) / 1000);
        }
    }

}
