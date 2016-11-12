using System.Collections.Generic;

namespace HastlayerTimingTester
{

    ///<summary>This is the base class for configuration. For more information, check the
    ///     <see cref="TimingTestConfig" /> subclass.</summary>
    abstract class TimingTestConfigBase
    {
        ///<summary>This is used for <see cref="DataTypes" />.</summary>
        public delegate string DataTypeFromSizeDelegate(int size, bool getFriendlyName);
        public string Name;
        public List<VhdlOp> Operators;
        public List<VhdlTemplateBase> VhdlTemplates;
        public List<int> InputSizes;
        public string Part;
        public List<DataTypeFromSizeDelegate> DataTypes;
        public string VivadoPath;
        public bool DebugMode;
        public decimal Frequency;
        public bool VivadoBatchMode;
        public bool ImplementDesign;
    }

}
