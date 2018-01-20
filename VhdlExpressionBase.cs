namespace HastlayerTimingTester
{
    /// <summary>
    /// Base class for generating VHDL expressions to test.
    /// </summary>
    public abstract class VhdlExpressionBase
    {
        /// <summary>
        /// Returns VHDL code.
        /// </summary>
        public abstract string GetVhdlCode(string[] inputs);

        /// <summary>
        /// Returns if the given expression is valid for the test case. This is useful e.g. for shift and 
        /// mul(a,pow(2,n)) where the test makes no sense if the shift amount is higher than the data type size.
        /// Its inputs are the variables that keep changing inside the for loop.
        /// </summary>
        public abstract bool IsValid(int inputSize, VhdlOp.DataTypeFromSizeDelegate inputDataTypeFunction, 
            VhdlTemplateBase vhdlTemplate);
    }
}
