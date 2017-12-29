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
    }
}
