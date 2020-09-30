namespace HastlayerTimingTester.Vhdl
{
    /// <summary>
    /// A VHDL template using the expression to test in an combinatorial logic design. This means that 
    /// there is no clock signal or flip-flops in the design, and the output is never fed back to the input.
    /// </summary>
    class VhdlTemplateComb : VhdlTemplateBase
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

        override public string Name => "comb";
    }
}
