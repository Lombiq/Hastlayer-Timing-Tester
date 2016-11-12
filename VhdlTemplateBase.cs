namespace HastlayerTimingTester
{

    ///<summary>VHDL templates contain the hardware project to be compiled. They consist of a VHDL and an XDC
    ///(constraints file) template, both of which will be used by Vivado.</summary>
    abstract class VhdlTemplateBase
    {
        public string VhdlTemplate { get; protected set; }
        public string XdcTemplate { get; protected set; }
        abstract public string Name { get; }
    }

}
