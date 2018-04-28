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
        /// Generates the VHDL code that will be substituted into the VHDL template.
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
        public OutputDataTypeDelegate OutputDataTypeFunction;

        /// <summary>
        /// Contains a list of functions that should be used for the data types the operation 
        /// should be tested for.
        /// </summary>
        public List<DataTypeFromSizeDelegate> DataTypes;

        /// <summary>Used for <see cref="DataTypes" />.</summary>
        public delegate string DataTypeFromSizeDelegate(int size, bool getFriendlyName);

        /// <summary>The VHDL templates that will be used for analysis.</summary>
        public List<VhdlTemplateBase> VhdlTemplates;


        public VhdlOp(
            VhdlExpressionBase vhdlExpression,
            string friendlyName,
            List<DataTypeFromSizeDelegate> dataTypes,
            OutputDataTypeDelegate outputDataTypeFunction,
            List<VhdlTemplateBase> vhdlTemplates)
        {
            VhdlExpression = vhdlExpression;
            FriendlyName = friendlyName;
            OutputDataTypeFunction = outputDataTypeFunction;
            DataTypes = dataTypes;
            VhdlTemplates = vhdlTemplates;
        }


        /// <summary>
        /// Used for generating the output data type based on template strings embedded in the function.
        /// See <see cref="OutputDataTypeFunction"/>.
        /// </summary>
        public delegate string OutputDataTypeDelegate(
            int inputSize,
            DataTypeFromSizeDelegate inputDataTypeFunction,
            bool getFriendlyName
        );

        /// <summary>Used if the output data type is the same as the input data type.</summary>
        public static string SameOutputDataType(
            int inputSize,
            DataTypeFromSizeDelegate inputDataTypeFunction,
            bool getFriendlyName) => inputDataTypeFunction(inputSize, getFriendlyName);

        /// <summary>
        /// Used for operators that strictly have boolean as their output data type (like all comparison operators).
        /// </summary>
        public static string ComparisonWithBoolOutput(
            int inputSize,
            DataTypeFromSizeDelegate inputDataTypeFunction,
            bool getFriendlyName) => "boolean";

        /// <summary>
        /// Used for operators whose output is the same type as their input, but with
        /// double data size (e.g. multiplication).
        /// </summary>
        public static string DoubleSizedOutput(
            int inputSize,
            DataTypeFromSizeDelegate inputDataTypeFunction,
            bool getFriendlyName) => inputDataTypeFunction(inputSize * 2, getFriendlyName);
    }
}
