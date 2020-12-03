namespace HastlayerTimingTester.Vhdl
{
    /// <summary>
    /// A VHDL template using the expression to test in a combinatorial logic design. This means that there is no clock
    /// signal or flip-flops in the design, and the output is never fed back to the input.
    /// </summary>
    internal class VhdlTemplateComb : VhdlTemplateBase
    {
        public VhdlTemplateComb()
        {
            VhdlTemplate =
@"entity tf_sample is
port(
    a1      : in %INTYPE%;
    a2      : in %INTYPE%;
    aout    : out %OUTTYPE%
);
end tf_sample;

architecture imp of tf_sample is begin
    aout <=  %EXPRESSION%;
end imp;";
            ExpressionInputs = new[] { "a1", "a2" };
            HasTimingConstraints = false;
        }

        public override string Name => "comb";
    }
}
