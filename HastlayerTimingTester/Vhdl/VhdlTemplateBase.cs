using System.Collections.Generic;

namespace HastlayerTimingTester.Vhdl
{
    /// <summary>
    /// VHDL templates contain the hardware project to be compiled after filling it with the required data.
    /// </summary>
    public abstract class VhdlTemplateBase
    {
        public string VhdlTemplate { get; protected set; }

        /// <summary>Gets or sets a value indicating whether the given template needs a constraints file.</summary>
        public bool HasTimingConstraints { get; protected set; }

        /// <summary>
        /// Gets name of the VHDL template, which will be used in e.g. the results and the test directory names.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Gets or sets the names of the input signals/variables in the template for the <see cref="VhdlOp"/>.
        /// </summary>
        public IReadOnlyList<string> ExpressionInputs { get; protected set; }
    }
}
