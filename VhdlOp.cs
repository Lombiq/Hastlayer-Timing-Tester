using System.Collections.Generic;

namespace HastlayerTimingTester
{

    /// <summary>
    /// VhdlOp provides data to fill a VHDL template with (see <see cref="VhdlString" /> and
    /// <see cref="OutputDataTypeFunction" />).
    /// </summary>
    public class VhdlOp
    {
        /// <summary>
        /// VhdlExpression generates the VHDL code that will be subsituted
        /// into the VHDL template.
        /// </summary>
        public VhdlExpressionBase VhdlExpression;

        /// <summary>
        /// FriendlyName will be used in directory names, where you cannot use special characters. E.g. for "+"
        /// a good FriendlyName is "add".
        /// </summary>
        public string FriendlyName;

        /// <summary>
        /// OutputDataTypeFunction can generate the output data type from the input data type and size. It
        /// allows us to handle VHDL operators that have different input and output data types.
        /// </summary>
        public OutputDataTypeDelegate OutputDataTypeFunction;

        /// <summary>
        /// DataTypes contain a list of functions that should be used for the data types the operation 
        /// should be tested for.
        /// </summary>
        public List<DataTypeFromSizeDelegate> DataTypes;

        /// <summary>This is used for <see cref="DataTypes" />.</summary>
        public delegate string DataTypeFromSizeDelegate(int size, bool getFriendlyName);

        /// <summary>VhdlTemplates: the VHDL templates that will be used for analysis.</summary>
        public List<VhdlTemplateBase> VhdlTemplates;

        public VhdlOp(VhdlExpressionBase vhdlExpression, string friendlyName, List<DataTypeFromSizeDelegate> dataTypes,
            OutputDataTypeDelegate outputDataTypeFunction, List<VhdlTemplateBase> vhdlTemplates)
        {
            this.VhdlExpression = vhdlExpression;
            this.FriendlyName = friendlyName;
            this.OutputDataTypeFunction = outputDataTypeFunction;
            this.DataTypes = dataTypes;
            this.VhdlTemplates = vhdlTemplates;
        }

        public delegate string OutputDataTypeDelegate(
            int inputSize,
            DataTypeFromSizeDelegate inputDataTypeFunction,
            bool getFriendlyName
        );

        /// <summary>SameOutputDataType is used if the output data type is the same as the input data type.</summary>
        public static string SameOutputDataType(
            int inputSize,
            DataTypeFromSizeDelegate inputDataTypeFunction,
            bool getFriendlyName
        )
        {
            return inputDataTypeFunction(inputSize, getFriendlyName);
        }

        /// <summary>
        /// ComparisonWithBoolOutput is used for operators that strictly have boolean as their output data type
        /// (like all comparison operators).
        /// </summary>
        public static string ComparisonWithBoolOutput(
            int inputSize,
            DataTypeFromSizeDelegate inputDataTypeFunction,
            bool getFriendlyName
        )
        {
            return "boolean";
        }

        /// <summary>
        /// DoubleSizedOutput is used for operators whose output is the same type as their input, but with
        /// double data size (e.g. multiplication).
        /// </summary>
        public static string DoubleSizedOutput(
            int inputSize,
            DataTypeFromSizeDelegate inputDataTypeFunction,
            bool getFriendlyName
        )
        {
            return inputDataTypeFunction(inputSize * 2, getFriendlyName);
        }
    }

}
