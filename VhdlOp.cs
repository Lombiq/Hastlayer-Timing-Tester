using System.Collections.Generic;

namespace HastlayerTimingTester
{

    /// <summary>
    /// Provides data to fill a VHDL template with (see <see cref="VhdlString" /> and
    /// <see cref="OutputDataTypeFunction" />).
    /// </summary>
    public class VhdlOp
    {
        /// <summary>
        /// Generates the VHDL code that will be subsituted into the VHDL template.
        /// </summary>
        public VhdlExpressionBase VhdlExpression;

        /// <summary>
        /// Will be used in directory names, where you cannot use special characters. E.g. for "+"
        /// a good FriendlyName is "add".
        /// </summary>
        public string FriendlyName;

        /// <summary>
        /// Can generate the output data type from the input data type and size. It
        /// allows us to handle VHDL operators that have different input and output data types.
        /// </summary>
        public OutputDataType OutputDataTypeFunction;

        /// <summary>
        /// Contains a list of functions that should be used for the data types the operation 
        /// should be tested for. TODO
        /// </summary>
        public List<DataTypes.Base> DataTypesList;

        /// <summary>The VHDL templates that will be used for analysis.</summary>
        public List<VhdlTemplateBase> VhdlTemplates;

        public VhdlOp(VhdlExpressionBase vhdlExpression, string friendlyName, List<DataTypes.Base> dataTypesList,
            DataTypes.Base outputDataType, List<VhdlTemplateBase> vhdlTemplates)
        {
            VhdlExpression = vhdlExpression;
            FriendlyName = friendlyName;
            OutputDataType = outputDataType;
            DataTypesList = dataTypesList;
            VhdlTemplates = vhdlTemplates;
        }

    }

}
