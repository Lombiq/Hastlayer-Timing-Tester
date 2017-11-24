using System.Collections.Generic;

namespace HastlayerTimingTester
{

    /// <summary>
    /// This is the base class for configuration. For more information, check the
    /// <see cref="TimingTestConfig" /> subclass.
    /// </summary>
    public abstract class TimingTestConfigBase
    {
        public string Name;
        public List<VhdlOp> Operators;
        public List<int> InputSizes;
        public string Part;
        public string VivadoPath;
        public bool DebugMode;
        public decimal Frequency;
        public bool VivadoBatchMode;
        public bool ImplementDesign;
        public FpgaVendorDriver Driver;
    }

}
