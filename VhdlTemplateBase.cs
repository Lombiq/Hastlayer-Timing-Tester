namespace HastlayerTimingTester
{
    /// <summary>
    /// VHDL templates contain the hardware project to be compiled after filling it with the required data. 
    /// </summary>
    abstract public class VhdlTemplateBase
    {
        public string VhdlTemplate { get; protected set; }

        /// <summary><see cref="HasTimingConstraints"/> is true if there the given template needs a constraints file.</summary>
        public bool HasTimingConstraints { get; protected set; }

        /// <summary>
        /// Name of the VHDL template, which will be used in e.g. the results and the test directory names.
        /// </summary>
        abstract public string Name { get; }

        /// <summary>
        /// Names of the input signals/variables in the template for the <see cref="VhdlOp"/>.
        /// </summary>
        public string[] ExpressionInputs { get; protected set; }
    }
}
