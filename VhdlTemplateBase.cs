namespace HastlayerTimingTester
{

    /// <summary>
    /// VHDL templates contain the hardware project to be compiled. They consist of a VHDL and an XDC
    /// (constraints file) template, both of which will be used by Vivado.
    /// </summary>
    abstract public class VhdlTemplateBase
    {
        public string VhdlTemplate { get; protected set; }
        public bool HasTimingConstraints { get; protected set; }
        abstract public string Name { get; }
    }

}
