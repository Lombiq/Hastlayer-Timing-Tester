namespace HastlayerTimingTester
{
    public class WrapSmartResizeVhdlExpression : VhdlExpressionBase
    {

        private VhdlExpressionBase _innerExpression;

        public WrapSmartResizeVhdlExpression(VhdlExpressionBase innerExpression)
        {
            _innerExpression = innerExpression;
        }

        public override string GetVhdlCode(string[] inputs, int inputSize) =>
            "SmartResize(" + _innerExpression.GetVhdlCode(inputs, inputSize) + ", " + inputSize.ToString() + ")";
        /// <summary>
        /// See <see cref="VhdlExpressionBase.IsValid"/>. Here we don't want to make any restriction on valid test 
        /// cases, as this class might generate test cases for a wide range of operators.
        /// </summary>
        public override bool IsValid(int inputSize, VhdlOp.DataTypeFromSizeDelegate inputDataTypeFunction,
            VhdlTemplateBase vhdlTemplate) => 
            _innerExpression.IsValid(inputSize, inputDataTypeFunction, vhdlTemplate); 

    }
}
