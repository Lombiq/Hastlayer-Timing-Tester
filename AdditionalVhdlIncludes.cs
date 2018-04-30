namespace HastlayerTimingTester
{
    class AdditionalVhdlIncludes
    {
        /// <summary>
        /// It is concatenated to the beginning of each UUT.vhd file.
        /// </summary>
        public const string Content = @"
library ieee;
use ieee.std_logic_1164.all;
use ieee.numeric_std.all;

package TypeConversion is
    function SmartResize(input: unsigned; size: natural) return unsigned;
    function SmartResize(input: signed; size: natural) return signed;
end TypeConversion;
        
package body TypeConversion is

    function SmartResize(input: unsigned; size: natural) return unsigned is
    begin
        if (size < input'LENGTH) then
            return input(size - 1 downto 0);
        else
            return resize(input, size);
        end if;
    end SmartResize;

    function SmartResize(input: signed; size: natural) return signed is
    begin
        if (size < input'LENGTH) then
            return input(size - 1 downto 0);
        else
            return resize(input, size);
        end if;
    end SmartResize;

end TypeConversion;

library ieee;
use ieee.std_logic_1164.all;
use ieee.numeric_std.all;
library work;
use work.TypeConversion.all;

";
    }
}
