namespace HastlayerTimingTester
{
    /// <summary>
    /// It is the base class for generating VHDL expressions to test.
    /// </summary>
    public abstract class VhdlExpressionBase
    {
        /// <summary>
        /// It returns VHDL code.
        /// </summary>
        public abstract string GetVhdlCode(string[] inputs);
    }
}
